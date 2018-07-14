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

using Android.Content;
using Flurry.Analytics;
using FormsSkiaBikeTracker.Droid.Services;
using FormsSkiaBikeTracker.Forms;
using FormsSkiaBikeTracker.Services.Interface;
using LRPFramework.Mvx.Views.Forms.Droid;
using LRPFramework.Services.Threading;
using MvvmCross.Binding;
using MvvmCross.IoC;
using MvxEntry = MvvmCross.Mvx;

namespace FormsSkiaBikeTracker.Droid
{
    public class Setup : LRPFormsDroidSetup<MvxApp, FormsApp>
    {
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

            MvxEntry.RegisterSingleton<IDocumentRoot>(MvxEntry.IocConstruct<DocumentRoot>);

            MvxEntry.CallbackWhenRegistered<MainThread>(SetupFlurry);
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