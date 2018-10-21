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
using Xamarin.Forms;
using FormsSkiaBikeTracker;
using MvvmCross.Forms.Platforms.Ios.Core;
using FormsSkiaBikeTracker.Forms;

[assembly: ResolutionGroupName(Constants.GeneralNamespace)]
namespace FormsSkiaBikeTracker.Ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, MvxApp, FormsApp>
    {
        protected override void RunAppStart(object hint = null)
        {
#if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
#endif

            Xamarin.Forms.Forms.Init();

            base.RunAppStart(hint);
        }
    }
}
