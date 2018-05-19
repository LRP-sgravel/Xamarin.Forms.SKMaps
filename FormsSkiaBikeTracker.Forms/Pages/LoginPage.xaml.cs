// **********************************************************************
// 
//   LoginPage.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FormsSkiaBikeTracker.ViewModels;
using MvvmCross.WeakSubscription;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Pages
{
    public partial class LoginPage
    {
        private IEnumerable<AthleteLoginWrapper> _athletesViewWrappers;
        public IEnumerable<AthleteLoginWrapper> AthletesViewWrappers
        {
            get => _athletesViewWrappers;
            set
            {
                if (AthletesViewWrappers != value)
                {
                    _athletesViewWrappers = value;
                    OnPropertyChanged();
                }
            }
        }

        private MvxNamedNotifyPropertyChangedEventSubscription<LoginViewModel> _viewModelSelectedAthleteChangedSubscription;
        private MvxNamedNotifyPropertyChangedEventSubscription<LoginViewModel> _viewModelAthletesChangedSubscription;

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NavigationPage.SetBackButtonTitle(this, string.Empty);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(BindingContext))
            {
                _viewModelAthletesChangedSubscription?.Dispose();
                _viewModelSelectedAthleteChangedSubscription?.Dispose();
                _viewModelAthletesChangedSubscription = null;
                _viewModelSelectedAthleteChangedSubscription = null;

                if (ViewModel != null)
                {
                    INotifyPropertyChanged npc = ViewModel;

                    RebuildAthletesViewWrappers();

                    _viewModelSelectedAthleteChangedSubscription = npc.WeakSubscribe<LoginViewModel>(nameof(ViewModel.SelectedAthlete),
                                                                                                     ViewModelSelectedAthleteChanged);
                    _viewModelAthletesChangedSubscription = npc.WeakSubscribe<LoginViewModel>(nameof(ViewModel.Athletes),
                                                                                              ViewModelAthletesChanged);
                }
            }
        }

        private void ViewModelSelectedAthleteChanged(object sender, EventArgs eventArgs)
        {
            AthleteLoginWrapper athleteWrapper = AthletesViewWrappers.FirstOrDefault(w => w.Athlete.Id == ViewModel.SelectedAthlete.Id);

            foreach (AthleteLoginWrapper wrapper in AthletesViewWrappers)
            {
                wrapper.IsExpanded = false;
            }

            if (athleteWrapper != null)
            {
                athleteWrapper.IsExpanded = true;
            }
        }

        private void ViewModelAthletesChanged(object sender, EventArgs eventArgs)
        {
            RebuildAthletesViewWrappers();
        }

        private void RebuildAthletesViewWrappers()
        {
            AthletesViewWrappers = ViewModel.Athletes
                                            .Select(a => new AthleteLoginWrapper { Athlete = a })
                                            .ToList();
        }

        private void SignupButtonClicked(object sender, EventArgs e)
        {
            NavigationPage.SetBackButtonTitle(this, ViewModel.LanguageBinder.GetText("Cancel"));
        }
    }
}
