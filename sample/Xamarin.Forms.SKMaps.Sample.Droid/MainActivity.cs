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
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Android.Runtime;

namespace Xamarin.Forms.SKMaps.Sample.Droid
{
    [Activity(Label = "SKMaps Demo",
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

            CrossCurrentActivity.Current.Init(this, bundle);

            base.OnCreate(bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}