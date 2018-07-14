// **********************************************************************
// 
//   Setup.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using Flurry.Analytics;
using FormsSkiaBikeTracker.Services.Interface;
using Foundation;
using LRPFramework.Mvx.Views.Forms.iOS;
using MvvmCross;
using MvvmCross.IoC;
using FormsSkiaBikeTracker.Forms;
using FormsSkiaBikeTracker.Ios.Services;
using LRPFramework.Services.Threading;

namespace FormsSkiaBikeTracker.Ios
{
    public class Setup : LRPFormsIosSetup<MvxApp, FormsApp>
    {
        protected override IMvxIocOptions CreateIocOptions()
        {
            return new MvxIocOptions()
            {
                PropertyInjectorOptions = MvxPropertyInjectorOptions.MvxInject
            };
        }
        
        protected override void InitializePlatformServices()
        {
            base.InitializePlatformServices();

            Mvx.LazyConstructAndRegisterSingleton<IDocumentRoot, DocumentRoot>();

            Mvx.CallbackWhenRegistered<MainThread>(SetupFlurry);
        }

        private void SetupFlurry()
        {
            bool enableCrashReporting = true;

#if DEBUG
            enableCrashReporting = false;
#endif

            FlurryAgent.SetDebugLogEnabled(true);                                         
            FlurryAgent.SetEventLoggingEnabled(true);
            FlurryAgent.SetCrashReportingEnabled(enableCrashReporting);
            FlurryAgent.SetAppVersion(NSBundle.MainBundle
                                              .ObjectForInfoDictionary("CFBundleVersion")
                                              .ToString());
            FlurryAgent.StartSession("BT7TJ7N85XMH4YMMH6ZC");
        }
    }
}