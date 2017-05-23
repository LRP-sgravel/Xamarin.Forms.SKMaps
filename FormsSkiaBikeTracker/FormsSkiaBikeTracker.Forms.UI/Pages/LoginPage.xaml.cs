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
using System.ComponentModel;
using LRPLib.Views.XForms;
using MvvmCross.Platform.WeakSubscription;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        private MvxWeakEventSubscription<LinearGradientBoxView> _signupPropertyChangedSubscription;

        public LoginPage()
        {
            InitializeComponent();

            _signupPropertyChangedSubscription = SignUpButtonBackground.WeakSubscribe("SizeChanged", SignUpBackgroundSizeChanged);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _signupPropertyChangedSubscription.Dispose();
            _signupPropertyChangedSubscription = null;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        private void SignUpBackgroundSizeChanged(object sender, EventArgs args)
        {
            const int BorderWidth = 4;

            SKRect buttonRect = SignUpButtonBackground.Bounds.ToSKRect();
            SKPath clipPath = new SKPath();
            float cornerSize = buttonRect.Height * 0.5f;

            clipPath.AddRoundedRect(buttonRect, cornerSize, cornerSize);
            buttonRect.Inflate(new SKSize(-BorderWidth, -BorderWidth));
            cornerSize = buttonRect.Height * 0.5f;
            clipPath.AddRoundedRect(buttonRect, cornerSize, cornerSize, SKPathDirection.CounterClockwise);

            SignUpButtonBackground.ClippingPath = clipPath;
        }
    }
}
