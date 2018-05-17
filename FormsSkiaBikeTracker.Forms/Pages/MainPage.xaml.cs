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

using System.ComponentModel;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Logging;
using MvvmCross.WeakSubscription;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.Pages
{
    [MvxContentPagePresentation(NoHistory = true)]
    public partial class MainPage
    {
        private object _locationChangedSubscriotion;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _locationChangedSubscriotion = null;

            if (ViewModel != null)
            {
                _locationChangedSubscriotion = ViewModel.WeakSubscribe(() => ViewModel.LastUserLocation,
                                                                       UserLocationChanged);
            }
        }

        private void UserLocationChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (ViewModel.UserLocationAcquired)
            {
                MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(ViewModel.LastUserLocation,
                                                                    MapControl.VisibleRegion.Radius));
            }
        }
    }
}
