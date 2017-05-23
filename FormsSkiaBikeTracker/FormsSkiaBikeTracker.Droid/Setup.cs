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
using Android.Content;
using Flurry.Analytics;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Droid.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Platform.IoC;
using FormsSkiaBikeTracker.Forms.UI.Views;
using LRPLib.Mvx.Droid;

namespace FormsSkiaBikeTracker.Droid
{
    public class Setup : LrpDroidSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
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

            return new LrpDroidDebugTrace(alwaysOutput);
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            LrpFormsAndroidPagePresenter presenter = new LrpFormsAndroidPagePresenter();
            Mvx.RegisterSingleton<IMvxViewPresenter>(presenter);

            return presenter;
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
            return new MvxIocOptions
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
            Context context = ApplicationContext;
            bool enableCrashReporting = true;

#if DEBUG
            enableCrashReporting = false;
#endif

            FlurryAgent.Init(ApplicationContext, "CGQSK9688VG9MFXMDRTS");
            FlurryAgent.SetLogEnabled(true);
            FlurryAgent.SetLogEvents(true);
            FlurryAgent.SetCaptureUncaughtExceptions(enableCrashReporting);
            FlurryAgent.SetVersionName(context.PackageManager
                                              .GetPackageInfo(context.PackageName, 0)
                                              .VersionName);
            FlurryAgent.OnStartSession(context);                                  
        }

    }
}