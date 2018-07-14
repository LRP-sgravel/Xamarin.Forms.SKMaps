// **********************************************************************
// 
//   MainActivity.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using Android.App;
using Android.OS;
using Android.Content.PM;
using MvvmCross.Forms.Platforms.Android.Views;

namespace FormsSkiaBikeTracker.Droid
{
    [Activity(Label = "FormsSkiaBikeTracker",
              Theme = "@style/MainTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : MvxFormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // Set the layout resources first
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate(bundle);
        }
    }
}