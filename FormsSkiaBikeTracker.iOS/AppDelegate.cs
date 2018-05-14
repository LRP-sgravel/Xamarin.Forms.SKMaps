// **********************************************************************
// 
//   AppDelegate.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using Foundation;
using UIKit;
using Xamarin.Forms;
using FormsSkiaBikeTracker;
using FormsSkiaBikeTracker.iOS;
using LRPFramework.Mvx;
using LRPFramework.Mvx.Views;
using LRPFramework.Mvx.Views.Forms;
using LRPFramework.Views;
using LRPFramework.Views.Forms;
using MvvmCross.Forms.Platforms.Ios.Core;
using TestApp.Forms;

[assembly: ResolutionGroupName(Constants.GeneralNamespace)]
namespace FormsSkiaBikeTracker.Ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, MvxApp, FormsApp>
    {
        UIWindow _window;

        protected override void RunAppStart(object hint = null)
        {
#if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
#endif

            Xamarin.Forms.Forms.Init();

            LRPFramework.LRPFramework.Init();
            LRPFrameworkMvx.Init();
            LRPFrameworkViews.Init();
            LRPFrameworkViewsForms.Init();
            LRPFrameworkMvxViews.Init();
            LRPFrameworkMvxViewsForms.Init();

            base.RunAppStart(hint);
        }
    }
}
