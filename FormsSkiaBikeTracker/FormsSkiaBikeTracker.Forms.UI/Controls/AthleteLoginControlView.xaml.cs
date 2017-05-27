// **********************************************************************
// 
//   AthleteLoginControlView.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System.ComponentModel;
using System.Reflection;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.WeakSubscription;
using Realms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class AthleteLoginControlView
    {
        public static readonly BindableProperty AthleteProperty = BindableProperty.Create(nameof(Athlete),
                                                                                       typeof(Athlete),
                                                                                       typeof(AthleteLoginControlView),
                                                                                       null,
                                                                                       BindingMode.OneWay,
                                                                                       null,
                                                                                       AthletePropertyChanged);

        public static readonly BindableProperty ExpandedProperty = BindableProperty.Create(nameof(Expanded),
                                                                                           typeof(bool),
                                                                                           typeof(AthleteLoginControlView),
                                                                                           false,
                                                                                           BindingMode.OneWay,
                                                                                           null,
                                                                                           ExpandedPropertyChanged);

        public bool Expanded
        {
            get { return (bool)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }


        public Athlete Athlete
        {
            get { return (Athlete)GetValue(AthleteProperty); }
            set { SetValue(AthleteProperty, value); }
        }

        private double _ClosedHeightRequest { get; }

        private AthleteLoginControlViewModel _internalViewModel;
        public AthleteLoginControlViewModel InternalViewModel
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
                            _internalViewModel.WeakSubscribe<AthleteLoginControlViewModel>(nameof(_internalViewModel.PropertyChanged), InternalVMPropertyChanged);
                    }
                    else
                    {
                        _internalPropertychangedSubscription = null;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private MvxNamedNotifyPropertyChangedEventSubscription<AthleteLoginControlViewModel> _internalPropertychangedSubscription;

        public AthleteLoginControlView()
        {
            InitializeComponent();

            _ClosedHeightRequest = HeightRequest;

            InternalViewModel = new AthleteLoginControlViewModel();
            InternalViewModel.Start();
        }

        private static void AthletePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AthleteLoginControlView view = bindable as AthleteLoginControlView;

            Athlete athlete = newValue as Athlete;
            PropertyInfo pi = typeof(Athlete).GetProperty("PicturePath");
            string propPath = athlete.PicturePath;
            string propReflectPath = pi.GetValue(athlete) as string;

            view.InternalViewModel.Athlete = newValue as Athlete;
        }
        
        private static void ExpandedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            AthleteLoginControlView view = bindable as AthleteLoginControlView;

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
            if (args.PropertyName == nameof(AthleteLoginControlViewModel.Athlete))
            {
                Athlete = InternalViewModel.Athlete;
            }
        }
    }
}
