// **********************************************************************
// 
//   MainPage.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.ComponentModel;
using MvvmCross.Base;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.WeakSubscription;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.Pages
{
    [MvxContentPagePresentation(NoHistory = true)]
    public partial class ActivityPage
    {
        private IDisposable _locationChangedSubscription;

        public ActivityPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _locationChangedSubscription = null;

            if (ViewModel != null)
            {
                _locationChangedSubscription = ViewModel.WeakSubscribe(() => ViewModel.LastUserLocation,
                                                                       UserLocationChanged);
            }
        }

        private void UserLocationChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Used to initally center the map on the user on first location acquisition
            CenterOnUser();
        }

        private void CenterOnUser(object sender, EventArgs e)
        {
            CenterOnUser();
        }

        private void CenterOnUser()
        {
            if (ViewModel.FirstLocationAcquired && MapControl?.VisibleRegion != null)
            {
                MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(ViewModel.LastUserLocation,
                                                                    MapControl.VisibleRegion.Radius));

                _locationChangedSubscription?.DisposeIfDisposable();
                _locationChangedSubscription = null;
            }
        }
    }
}
