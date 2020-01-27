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
using Xamarin.Forms.SKMaps.Sample.Android.Services;
using Xamarin.Forms.SKMaps.Sample.Forms;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using MvvmCross.Base;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.IoC;
using MvxEntry = MvvmCross.Mvx;

namespace Xamarin.Forms.SKMaps.Sample.Android
{
    public class Setup : MvxFormsAndroidSetup<MvxApp, FormsApp>
    {
        protected override IMvxIocOptions CreateIocOptions()
        {
            return new MvxIocOptions
            {
                PropertyInjectorOptions = MvxPropertyInjectorOptions.MvxInject
            };
        }
        
        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();

            MvxEntry.IoCProvider.RegisterSingleton<IDocumentRoot>(MvxEntry.IoCProvider.IoCConstruct<DocumentRoot>);

            MvxEntry.IoCProvider.CallbackWhenRegistered<IMvxMainThreadAsyncDispatcher>(SetupFlurry);
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