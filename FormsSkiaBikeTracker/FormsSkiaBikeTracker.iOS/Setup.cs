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
using UIKit;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Platform.Platform;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.Platform.IoC;
using FormsSkiaBikeTracker.Forms.UI.Views;
using Foundation;
using LRPLib.Mvx.iOS;

namespace FormsSkiaBikeTracker.iOS
{
    public class Setup : LrpIosSetup
    {
        public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            bool alwaysOutput = false;

#if DEBUG
            alwaysOutput = true;
#endif

            return new LrpIosDebugTrace(alwaysOutput);
        }

        protected override IMvxIosViewPresenter CreatePresenter()
        {
            Xamarin.Forms.Forms.Init();

            return new LrpFormsIosPagePresenter(Window, new MvxFormsApp());
        }

        protected override IEnumerable<Assembly> GetViewAssemblies()
        {
            var result = base.GetViewAssemblies();

            return result.Append(typeof(LoadingPage).Assembly).ToList();
        }

        protected override IMvxNameMapping CreateViewToViewModelNaming()
        {
            return new MvxPostfixAwareViewToViewModelNameMapping("View", "Page");
        }

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