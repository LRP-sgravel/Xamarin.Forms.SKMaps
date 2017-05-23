// **********************************************************************
// 
//   UserLoginControlView.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using System;
using System.ComponentModel;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using MvvmCross.Platform.WeakSubscription;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public partial class UserLoginControlView
    {
        public static readonly BindableProperty UserProperty = BindableProperty.Create(nameof(User), typeof(User), typeof(UserLoginControlView), null, BindingMode.OneWay, null, UserPropertyChanged);

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        private UserLoginControlViewModel _internalViewModel;
        public UserLoginControlViewModel InternalViewModel
        {
            get { return _internalViewModel; }
            set
            {
                if (value != InternalViewModel)
                {
                    _internalViewModel = value;

                    if (_internalViewModel != null)
                    {
                        _internalPropertychangedSubscription =
                            _internalViewModel.WeakSubscribe<UserLoginControlViewModel, PropertyChangedEventArgs>("PropertyChanged", InternalVMPropertyChanged);
                    }
                    else
                    {
                        _internalPropertychangedSubscription = null;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private MvxWeakEventSubscription<UserLoginControlViewModel, PropertyChangedEventArgs> _internalPropertychangedSubscription;

        public UserLoginControlView()
        {
            InitializeComponent();

            _internalViewModel = new UserLoginControlViewModel();
            InternalViewModel.Start();
        }

        private static void UserPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            UserLoginControlView view = bindable as UserLoginControlView;

            view.InternalViewModel.User = newvalue as User;
        }

        private void InternalVMPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(UserLoginControlViewModel.User))
            {
                User = InternalViewModel.User;
            }
        }
    }
}
