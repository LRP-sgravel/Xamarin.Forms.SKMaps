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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flurry.Analytics;
using FormsSkiaBikeTracker.Forms.Pages;
using FormsSkiaBikeTracker.iOS.Services;
using FormsSkiaBikeTracker.Services.Interface;
using Foundation;
using LRPFramework.Mvx.Views.Forms.iOS;
using MvvmCross;
using MvvmCross.IoC;
using TestApp.Forms;

namespace FormsSkiaBikeTracker.iOS
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

            Mvx.RegisterSingleton<IDocumentRoot>(Mvx.IocConstruct<DocumentRoot>);
            SetupFlurry();
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