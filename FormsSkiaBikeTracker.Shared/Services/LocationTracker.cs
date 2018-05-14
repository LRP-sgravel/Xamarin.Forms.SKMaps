// **********************************************************************
// 
//   LocationTracker.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin.Location;

namespace FormsSkiaBikeTracker.Services
{
    public class LocationTracker : ILocationTracker
    {
        public event EventHandler<LocationMovedEventArgs> Moved;

        [MvxInject]
        public IMvxLocationWatcher LocationService { get; set; }

        public bool IsTracking => LocationService.Started;
        public MvxGeoLocation Location => LocationService.CurrentLocation;

        public void Start(int refreshMovementMeters = 30, int refreshSeconds = 15, bool foreground = true)
        {
            // Stop if already started
            Pause();

            if (!LocationService.Started)
            {
                MvxLocationOptions options = new MvxLocationOptions();

                options.Accuracy = MvxLocationAccuracy.Fine;
                options.MovementThresholdInM = refreshMovementMeters;
                options.TimeBetweenUpdates = TimeSpan.FromSeconds(refreshSeconds);

                if (foreground)
                {
                    options.TrackingMode = MvxLocationTrackingMode.Foreground;
                }
                else
                {
                    options.TrackingMode = MvxLocationTrackingMode.Background;
                }

                LocationService.Start(options, OnLocationUpdated, OnWatcherError);
            }
        }

        public void Pause()
        {
            if (LocationService.Started)
            {
                LocationService.Stop();
            }
        }

        private void OnLocationUpdated(MvxGeoLocation newLocation)
        {
            Moved?.Invoke(this,
                          new LocationMovedEventArgs
                          {
                              Location = newLocation
                          });
        }

        private void OnWatcherError(MvxLocationError error)
        {
            MvxLog.Instance.Log(MvxLogLevel.Warn, () => $"Error tracking location - {error.Code}");
        }
    }
}
