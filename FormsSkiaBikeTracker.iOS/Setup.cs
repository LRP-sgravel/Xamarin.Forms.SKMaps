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
using MvvmCross;
using MvvmCross.IoC;
using FormsSkiaBikeTracker.Forms;
using FormsSkiaBikeTracker.Ios.Services;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.Base;
using MvvmCross.Plugin.Json;

namespace FormsSkiaBikeTracker.Ios
{
    public class Setup : MvxFormsIosSetup<MvxApp, FormsApp>
    {
        protected override IMvxIocOptions CreateIocOptions()
        {
            return new MvxIocOptions()
            {
                PropertyInjectorOptions = MvxPropertyInjectorOptions.MvxInject
            };
        }

        protected override void InitializeFirstChance()
        {
            Mvx.RegisterSingleton<IMvxJsonConverter>(new MvxJsonConverter());

            base.InitializeFirstChance();
        }

        protected override void InitializePlatformServices()
        {
            base.InitializePlatformServices();

            Mvx.LazyConstructAndRegisterSingleton<IDocumentRoot, DocumentRoot>();

            Mvx.CallbackWhenRegistered<IMvxMainThreadAsyncDispatcher>(SetupFlurry);
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