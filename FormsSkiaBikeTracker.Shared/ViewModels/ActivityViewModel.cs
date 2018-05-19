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
using FormsSkiaBikeTracker.Extensions.Realm;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using LRPFramework.Extensions;
using LRPFramework.Mvx.ViewModels;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.WeakSubscription;
using Realms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Overlays.Extensions;

namespace FormsSkiaBikeTracker.ViewModels
{
    [MvxContentPagePresentation(NoHistory = true, Animated = true)]
    public class ActivityViewModel : LRPViewModel<string>
    {
        [MvxInject]
        public ILocationTracker LocationTracker { get; set; }

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

        private double _totalDistance;
        public double TotalDistance
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

        private object _locationChangedSubscription;
        private object _recordingActivitySubscription;

        public override void Start()
        {
            base.Start();

            _locationChangedSubscription = LocationTracker.WeakSubscribe<ILocationTracker, LocationMovedEventArgs>(nameof(LocationTracker.Moved),
                                                                                                                   UserLocationUpdated);
        }

        public override void Prepare(string athleteId)
        {
            base.Prepare(athleteId);

            Athlete = Realm.GetInstance(RealmConstants.RealmConfiguration)
                           .Find<Athlete>(athleteId);
        }

        private async void ToggleRecording()
        {
            if (_Recorder == null)
            {
                _Recorder = Mvx.Resolve<IRouteRecorder>();

                try
                {
                    ActivityRoute = _Recorder.Start();
                    _CurrentActivity = await CreateNewActivityFromRecording(ActivityRoute, Athlete)
                                           .ConfigureAwait(false);
                    

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

        private async Task<Activity> CreateNewActivityFromRecording(IMutableRoute activityRoute, Athlete athlete)
        {
            Activity result = null;

            TotalDistance = 0;

            Realm realmInstance = Realm.GetInstance(RealmConstants.RealmConfiguration);
            realmInstance.Write(() =>
                           {
                               result = new Activity();
                               result.StartTime = DateTimeOffset.Now;
                               result.Route = new ActivityRoute();

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

            FirstLocationAcquired = true;
            _LastLocationTime = now;
            LastUserLocation = newLocation;
        }

        private void UpdateActivityStatistics(Position location, DateTimeOffset locationTime)
        {
            if (_LastLocationTime > DateTimeOffset.MinValue)
            {
                Distance distanceTravelled = LastUserLocation.FastDistanceTo(location);
                double timeElapsed = locationTime.TimeOfDay.TotalHours - _LastLocationTime.TimeOfDay.TotalHours;

                CurrentSpeed = distanceTravelled.ToDistanceUnit(Athlete.DistanceUnit) / timeElapsed;

                if (_CurrentActivity != null)
                {
                    Realm realmInstance = Realm.GetInstance(RealmConstants.RealmConfiguration);

                    TotalDistance += distanceTravelled.ToDistanceUnit(Athlete.DistanceUnit);

                    realmInstance.Write(() =>
                        {
                            Activity activity = realmInstance.Find<Activity>(_CurrentActivity.Id);

                            if (activity.Statistics == null)
                            {
                                activity.Statistics = new ActivityStatistics();
                            }

                            activity.Statistics.DistanceMeters += distanceTravelled.Meters;
                        });
                }
            }
        }

        private void OnNewPointRecorded(object sender, PointsAddedEventArgs args)
        {
            SavePointsToActivityAsync(_CurrentActivity.Id, args.NewPoints)
                .ForgetAndCatch();
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
