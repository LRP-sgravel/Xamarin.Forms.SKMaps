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
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using LRPLib.Views.XForms;
using MvvmCross.Platform.WeakSubscription;
using PropertyChanged;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        [ImplementPropertyChanged]
        public class UserLoginWrapper
        {
            public User User { get; set; }
            public bool IsExpanded { get; set; }
        }
        
        public IEnumerable<UserLoginWrapper> UsersViewWrappers { get; set; }
        public UserLoginWrapper SelectedUser
        {
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.SelectedUser = value?.User;
                }
            }
        }

        private MvxNamedNotifyPropertyChangedEventSubscription<LoginViewModel> _viewModelSelectedUserChangedSubscription;
        private MvxNamedNotifyPropertyChangedEventSubscription<LoginViewModel> _viewModelUsersChangedSubscription;

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(BindingContext))
            {
                _viewModelUsersChangedSubscription?.Dispose();
                _viewModelSelectedUserChangedSubscription?.Dispose();
                _viewModelUsersChangedSubscription = null;
                _viewModelSelectedUserChangedSubscription = null;

                if (ViewModel != null)
                {
                    INotifyPropertyChanged npc = ViewModel;

                    RebuildUsersViewWrappers();

                    _viewModelSelectedUserChangedSubscription = npc.WeakSubscribe<LoginViewModel>("SelectedUser", ViewModelSelectedUserChanged);
                    _viewModelUsersChangedSubscription = npc.WeakSubscribe<LoginViewModel>("Users", ViewModelUsersChanged);
                }
            }
        }

        private void ViewModelSelectedUserChanged(object sender, EventArgs eventArgs)
        {
            UserLoginWrapper userWrapper = UsersViewWrappers.FirstOrDefault(w => w.User == ViewModel.SelectedUser);

            foreach (UserLoginWrapper wrapper in UsersViewWrappers)
            {
                wrapper.IsExpanded = false;
            }

            if (userWrapper != null)
            {
                userWrapper.IsExpanded = true;
            }
        }

        private void ViewModelUsersChanged(object sender, EventArgs eventArgs)
        {
            RebuildUsersViewWrappers();
        }

        private void RebuildUsersViewWrappers()
        {
            UsersViewWrappers = ViewModel.Users
                                         .Select(u => new UserLoginWrapper { User = u })
                                         .ToList();
        }
    }
}
