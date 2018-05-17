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

namespace FormsSkiaBikeTracker.ViewModels
{
    [MvxContentPagePresentation(NoHistory = true, Animated = true)]
    public class MainViewModel : LRPViewModel<string>
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


        private IRouteRecorder _Recorder { get; set; }
        private Activity _CurrentActivity { get; set; }

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

        private IMvxCommand _toggleRecordingCommand;
        public IMvxCommand ToggleRecordingCommand => _toggleRecordingCommand ?? (_toggleRecordingCommand = new MvxCommand(ToggleRecording));

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

            Athlete = Realm.GetInstance()
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
                catch (Exception e)
                {
                    MvxLog.Instance.Log(MvxLogLevel.Error, () => "Failed to create new activity");
                }
            }
            else
            {
                _recordingActivitySubscription = null;
                _CurrentActivity = null;
                _Recorder.Stop();
                _Recorder = null;
            }
        }

        private async Task<Activity> CreateNewActivityFromRecording(IMutableRoute activityRoute, Athlete athlete)
        {
            Activity result = null;

            await Realm.GetInstance()
                       .WriteAsync(realmInstance =>
                           {
                               result = new Activity();
                               result.StartTime = DateTimeOffset.Now;
                               result.Route = new ActivityRoute();

                               athlete.Activities.Add(result);
                           })
                       .ConfigureAwait(false);

            await SavePointsToActivity(result.Id, activityRoute.Points.ToList())
                .ConfigureAwait(false);

            return result;
        }

        private void UserLocationUpdated(object sender, LocationMovedEventArgs args)
        {
            FirstLocationAcquired = true;
            LastUserLocation = new Position(args.Location.Coordinates.Latitude,
                                            args.Location.Coordinates.Longitude);
        }

        private void OnNewPointRecorded(object sender, PointsAddedEventArgs args)
        {
            SavePointsToActivity(_CurrentActivity.Id, args.NewPoints)
                .ForgetAndCatch();
        }

        private Task SavePointsToActivity(long activityId, IEnumerable<Position> points)
        {
            return Realm.GetInstance()
                        .WriteAsync(realmInstance =>
                            {
                                Activity activity = realmInstance.Find<Activity>(activityId);
                                foreach (RoutePoint newPoint in points.Select(p => p.ToRoutePoint()))
                                {
                                    activity.Route.RealmPoints.Add(newPoint);
                                }
                            });
        }
    }
}
