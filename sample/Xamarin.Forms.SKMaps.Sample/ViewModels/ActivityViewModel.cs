// **********************************************************************
// 
//   MainViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms.SKMaps.Sample.Extensions;
using Xamarin.Forms.SKMaps.Sample.Extensions.Realm;
using Xamarin.Forms.SKMaps.Sample.Models;
using Xamarin.Forms.SKMaps.Sample.Services;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using MvvmCross.WeakSubscription;
using Realms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.SKMaps.Extensions;

namespace Xamarin.Forms.SKMaps.Sample.ViewModels
{
    public class ActivityViewModel : MvxViewModel<string>
    {
        private const double DefaultStaleTimerMs = 3000;

        [MvxInject]
        public ILocationTracker LocationTracker { get; set; }

        [MvxInject]
        public IResourceLocator ResourceLocator { get; set; }

        public LanguageBinder LanguageBinder { get; private set; }

        private Athlete _athlete;
        public Athlete Athlete
        {
            get => _athlete;
            set
            {
                if (Athlete != value)
                {
                    _athlete = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Position _lastUserLocation;
        public Position LastUserLocation
        {
            get => _lastUserLocation;
            set
            {
                if (LastUserLocation != value)
                {
                    _lastUserLocation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMutableRoute _activityRoute;
        public IMutableRoute ActivityRoute
        {
            get => _activityRoute;
            set
            {
                if (ActivityRoute != value)
                {
                    _activityRoute = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _firstLocationAcquired;
        public bool FirstLocationAcquired
        {
            get => _firstLocationAcquired;
            set
            {
                if (FirstLocationAcquired != value)
                {
                    _firstLocationAcquired = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _currentSpeed;
        public double CurrentSpeed
        {
            get => _currentSpeed;
            set
            {
                if (CurrentSpeed != value)
                {
                    _currentSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Distance _totalDistance;
        public Distance TotalDistance
        {
            get => _totalDistance;
            set
            {
                if (TotalDistance != value)
                {
                    _totalDistance = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsActivityRunning => _CurrentActivity != null;

        private IMvxCommand _toggleRecordingCommand;
        public IMvxCommand ToggleRecordingCommand => _toggleRecordingCommand ?? (_toggleRecordingCommand = new MvxCommand(ToggleRecording));
        
        private IRouteRecorder _Recorder { get; set; }
        private Activity _CurrentActivity { get; set; }
        private DateTimeOffset _LastLocationTime { get; set; } = DateTimeOffset.MinValue;
        private TimerWrapper _CurrentStaleTimer { get; set; }

        private IDisposable _locationChangedSubscription;
        private IDisposable _recordingActivitySubscription;
        private IDisposable _staleTimerSubscription;

        public override void Start()
        {
            base.Start();

            LanguageBinder = new LanguageBinder(ResourceLocator.ResourcesNamespace,
                                                GetType().FullName.Replace(ResourceLocator.ResourcesNamespace + ".", string.Empty),
                                                false);

            _locationChangedSubscription = LocationTracker.WeakSubscribe<ILocationTracker, LocationMovedEventArgs>(nameof(LocationTracker.Moved),
                                                                                                                   UserLocationUpdated);
            if (LocationTracker.IsTracking)
            {
                UserLocationUpdated(this, new LocationMovedEventArgs {Location = LocationTracker.Location});
            }
        }

        public override void Prepare(string athleteId)
        {
            Athlete = Realm.GetInstance(RealmConstants.RealmConfiguration)
                           .Find<Athlete>(athleteId);
        }

        private void ToggleRecording()
        {
            if (_Recorder == null)
            {
                _Recorder = Mvx.IoCProvider.Resolve<IRouteRecorder>();

                try
                {
                    ActivityRoute = _Recorder.Start();
                    _CurrentActivity = CreateNewActivityFromRecording(ActivityRoute, Athlete);

                    _recordingActivitySubscription = ActivityRoute.WeakSubscribe<IMutableRoute, PointsAddedEventArgs>(nameof(ActivityRoute.PointsAdded),
                                                                                                                      OnNewPointRecorded);
                }
                catch(Exception e)
                {
                    MvxLog.Instance.Log(MvxLogLevel.Error, () => "Failed to create new activity", e);
                }
            }
            else
            {
                _recordingActivitySubscription = null;
                _CurrentActivity = null;
                _Recorder.Stop();
                _Recorder = null;
            }

            RaisePropertyChanged(nameof(IsActivityRunning));
        }

        private Activity CreateNewActivityFromRecording(IMutableRoute activityRoute, Athlete athlete)
        {
            Activity result = null;

            TotalDistance = Distance.FromMeters(0);

            Realm realmInstance = Realm.GetInstance(RealmConstants.RealmConfiguration);
            realmInstance.Write(() =>
                           {
                               result = new Activity();
                               result.StartTime = DateTimeOffset.Now;
                               result.Route = new ActivityRoute();
                               result.Statistics = new ActivityStatistics();

                               athlete.Activities.Add(result);

                               SavePointsToActivity(result, activityRoute.Points.ToList());
                           });

            return result;
        }

        private void UserLocationUpdated(object sender, LocationMovedEventArgs args)
        {
            Position newLocation = new Position(args.Location.Coordinates.Latitude,
                                                args.Location.Coordinates.Longitude);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            UpdateActivityStatistics(newLocation, now);
            StartStaleTimer();

            _LastLocationTime = now;
            LastUserLocation = newLocation;
            FirstLocationAcquired = true;
        }

        private void StartStaleTimer()
        {
            // Release previous timer
            _CurrentStaleTimer?.Stop();
            _CurrentStaleTimer?.Dispose();
            _staleTimerSubscription?.Dispose();
            
            // Setup new one with interval
            _CurrentStaleTimer = new TimerWrapper(DefaultStaleTimerMs);
            _CurrentStaleTimer.AutoReset = false;
            _staleTimerSubscription = _CurrentStaleTimer.WeakSubscribe<TimerWrapper, ElapsedEventArgs>(nameof(_CurrentStaleTimer.Elapsed), OnStaleTimerElapsed);
            _CurrentStaleTimer.Start();
        }

        private void OnStaleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentSpeed = 0;
        }

        private async void UpdateActivityStatistics(Position location, DateTimeOffset locationTime)
        {
            if (_LastLocationTime > DateTimeOffset.MinValue)
            {
                Distance distanceTravelled = LastUserLocation.FastDistanceTo(location);
                double timeElapsed = locationTime.TimeOfDay.TotalHours - _LastLocationTime.TimeOfDay.TotalHours;

                CurrentSpeed = distanceTravelled.ToDistanceUnit(Athlete.DistanceUnit) / timeElapsed;

                if (IsActivityRunning)
                {
                    TotalDistance = TotalDistance.Add(distanceTravelled);

                    try
                    {
                        await SaveStatisticsToRealmAsync(TotalDistance).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        MvxLog.Instance.Log(MvxLogLevel.Error, () => "Failed to save statistics to Mux", e);
                    }
                }
            }
        }

        private Task SaveStatisticsToRealmAsync(Distance totalDistance)
        {
            return Realm.GetInstance(RealmConstants.RealmConfiguration)
                        .WriteAsync(realmInstance =>
                            {
                                Activity activity = realmInstance.Find<Activity>(_CurrentActivity.Id);

                                activity.Statistics.DistanceMeters = totalDistance.Meters;
                            });
        }

        private void OnNewPointRecorded(object sender, PointsAddedEventArgs args)
        {
            SavePointsToActivityAsync(_CurrentActivity.Id, args.NewPoints).ForgetAndCatch();
        }

        private Task SavePointsToActivityAsync(string activityId, IEnumerable<Position> points)
        {
            return Realm.GetInstance(RealmConstants.RealmConfiguration)
                        .WriteAsync(realmInstance =>
                            {
                                List<Activity> activities = realmInstance.All<Activity>().ToList();
                                Activity activity = realmInstance.Find<Activity>(activityId);

                                if (activity != null)
                                {
                                    SavePointsToActivity(activity, points);
                                }
                            });
        }

        private void SavePointsToActivity(Activity activity, IEnumerable<Position> points)
        {
            foreach (RoutePoint newPoint in points.Select(p => p.ToRoutePoint()))
            {
                activity.Route.RealmPoints.Add(newPoint);
            }
        }
    }
}
