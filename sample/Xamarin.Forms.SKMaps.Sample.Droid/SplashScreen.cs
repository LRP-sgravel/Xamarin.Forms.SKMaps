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

using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Xamarin.Forms.SKMaps.Sample.Forms;
using MvvmCross.Forms.Platforms.Android.Views;
using Acr.UserDialogs;
using MvvmCross.Platforms.Android;
using MvxEntry = MvvmCross.Mvx;

namespace Xamarin.Forms.SKMaps.Sample.Android
{
    [Activity(Label = "SKMaps Demo",
              MainLauncher = true,
              Icon = "@drawable/icon",
              Theme = "@style/AppTheme.Splash",
              NoHistory = true)]
    public class SplashScreen : MvxFormsSplashScreenAppCompatActivity<Setup, MvxApp, FormsApp>
    {
        public SplashScreen() : base(Resource.Layout.splashscreen)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            UserDialogs.Init(() => MvxEntry.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>().Activity);

            base.OnCreate(bundle);
        }

        protected override Task RunAppStartAsync(Bundle bundle)
        {
            StartActivity(typeof(MainActivity));

            return base.RunAppStartAsync(bundle);
        }
    }
}