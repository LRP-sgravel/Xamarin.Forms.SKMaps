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
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using LRPLib.Mvx.ViewModels;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Plugins.Location;
using Realms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.ViewModels
{
    public class MainViewModel : LrpViewModel
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

        private bool _userLocationAcquired;
        public bool UserLocationAcquired
        {
            get => _userLocationAcquired;
            set
            {
                if (UserLocationAcquired != value)
                {
                    _userLocationAcquired = value;
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

        private object _locationChangedSubscription;

        public MainViewModel()
        {
        }

        public void Init(string athleteId)
        {
            Athlete = Realm.GetInstance()
                           .Find<Athlete>(athleteId);
        }

        public override void Start()
        {
            base.Start();

            _locationChangedSubscription = new MvxWeakEventSubscription<ILocationTracker, MvxGeoLocation>(LocationTracker,
                                                                                                          nameof(LocationTracker.Moved),
                                                                                                          UserLocationUpdated);
        }

        private void UserLocationUpdated(object sender, MvxGeoLocation location)
        {
            UserLocationAcquired = true;
            LastUserLocation = new Position(location.Coordinates.Latitude,
                                            location.Coordinates.Longitude);
        }
    }
}

