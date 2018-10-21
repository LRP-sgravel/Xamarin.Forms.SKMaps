// **********************************************************************
// 
//   SplashScreen.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using Android.App;
using Android.OS;
using FormsSkiaBikeTracker.Forms;
using MvvmCross.Forms.Platforms.Android.Views;
using Acr.UserDialogs;
using MvvmCross.Platforms.Android;
using MvxEntry = MvvmCross.Mvx;

namespace FormsSkiaBikeTracker.Droid
{
    [Activity(Label = "FormsSkiaBikeTracker",
              MainLauncher = true,
              Icon = "@drawable/icon",
              Theme = "@style/AppTheme.Splash",
              NoHistory = true)]
    public class SplashScreen : MvxFormsSplashScreenAppCompatActivity<Setup, MvxApp, FormsApp>
    {
        public SplashScreen() : base(Resource.Layout.SplashScreen)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            UserDialogs.Init(() => MvxEntry.Resolve<IMvxAndroidCurrentTopActivity>().Activity);

            base.OnCreate(bundle);
        }

        protected override void RunAppStart(Bundle bundle)
        {
            StartActivity(typeof(MainActivity));

            base.RunAppStart(bundle);
        }
    }
}