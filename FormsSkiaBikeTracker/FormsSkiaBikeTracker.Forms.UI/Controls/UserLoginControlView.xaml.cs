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

using System.ComponentModel;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using MvvmCross.Platform.WeakSubscription;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class UserLoginControlView
    {
        public static readonly BindableProperty UserProperty = BindableProperty.Create(nameof(User),
                                                                                       typeof(User),
                                                                                       typeof(UserLoginControlView),
                                                                                       null,
                                                                                       BindingMode.OneWay,
                                                                                       null,
                                                                                       UserPropertyChanged);

        public static readonly BindableProperty ExpandedProperty = BindableProperty.Create(nameof(Expanded),
                                                                                           typeof(bool),
                                                                                           typeof(UserLoginControlView),
                                                                                           false,
                                                                                           BindingMode.OneWay,
                                                                                           null,
                                                                                           ExpandedPropertyChanged);

        public bool Expanded
        {
            get { return (bool)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }


        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        private double _ClosedHeightRequest { get; }

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
                            _internalViewModel.WeakSubscribe<UserLoginControlViewModel>("PropertyChanged", InternalVMPropertyChanged);
                    }
                    else
                    {
                        _internalPropertychangedSubscription = null;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private MvxNamedNotifyPropertyChangedEventSubscription<UserLoginControlViewModel> _internalPropertychangedSubscription;

        public UserLoginControlView()
        {
            InitializeComponent();

            _ClosedHeightRequest = HeightRequest;

            InternalViewModel = new UserLoginControlViewModel();
            InternalViewModel.Start();
        }

        private static void UserPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            UserLoginControlView view = bindable as UserLoginControlView;

            view.InternalViewModel.User = newvalue as User;
        }
        
        private static void ExpandedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            UserLoginControlView view = bindable as UserLoginControlView;

            view.SetupPasswordLayout();
        }

        private void SetupPasswordLayout()
        {
            if (Expanded)
            {
                AnimateHeightRequest(PasswordLayout.Height + 1);
            }
            else
            {
                AnimateHeightRequest(-PasswordLayout.Height - 1);
            }
        }

        private void AnimateHeightRequest(double offset)
        {
            double startViewHeight = HeightRequest;
            double startPasswordHeight = HeightRequest;

            this.Animate($"Animation-{offset}",
                         p =>
                         {
                             HeightRequest = startViewHeight + p;
                             PasswordLayout.HeightRequest = startPasswordHeight + p;
                             InvalidateMeasure();
                         },
                         0,
                         offset,
                         16,
                         250,
                         Easing.CubicInOut);
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
