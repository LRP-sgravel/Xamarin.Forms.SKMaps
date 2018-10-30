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
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin.Location;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Xamarin.Forms.SKMaps.Sample.Services
{
    public class LocationTracker : ILocationTracker
    {
        public event EventHandler<LocationMovedEventArgs> Moved;

        [MvxInject]
        public IMvxLocationWatcher LocationService { get; set; }

        public bool IsTracking => LocationService.Started;
        public MvxGeoLocation Location => LocationService.CurrentLocation;

        public async Task Start(int refreshMovementMeters = 30, int refreshSeconds = 15, bool foreground = true)
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

                if (await GetLocationPermissions())
                {
                    LocationService.Start(options, OnLocationUpdated, OnWatcherError);
                }
            }
        }

        private async Task<bool> GetLocationPermissions()
        {
            Permission permissionType = Permission.LocationAlways;
            PermissionStatus currentStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(permissionType)
                                                                           .ConfigureAwait(false);

            if (currentStatus != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permissionType)
                                                  .ConfigureAwait(false))
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig
                                                          {
                                                              Message = "We need your permission to use your location"
                                                          })
                                              .ConfigureAwait(false);
                }

                Dictionary<Permission, PermissionStatus> result = await CrossPermissions.Current.RequestPermissionsAsync(permissionType)
                                                                                                .ConfigureAwait(false);

                if (result.ContainsKey(permissionType))
                {
                    currentStatus = result[permissionType];
                }
                else
                {
                    currentStatus = PermissionStatus.Unknown;
                }
            }

            return currentStatus == PermissionStatus.Granted;
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
