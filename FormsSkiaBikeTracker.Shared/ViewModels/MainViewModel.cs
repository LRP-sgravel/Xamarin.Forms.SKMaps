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

using System.Threading.Tasks;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Services.Interface;
using LRPFramework.Mvx.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.IoC;
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

        public override void Prepare(string athleteId)
        {
            base.Prepare(athleteId);

            Athlete = Realm.GetInstance()
                           .Find<Athlete>(athleteId);
        }

        public override Task Initialize()
        {
            _locationChangedSubscription = LocationTracker.WeakSubscribe<ILocationTracker, LocationMovedEventArgs>(nameof(LocationTracker.Moved),
                                                                                                                   UserLocationUpdated);

            return base.Initialize();
        }

        private void UserLocationUpdated(object sender, LocationMovedEventArgs args)
        {
            UserLocationAcquired = true;
            LastUserLocation = new Position(args.Location.Coordinates.Latitude,
                                            args.Location.Coordinates.Longitude);
        }
    }
}
