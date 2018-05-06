// **********************************************************************
// 
//   App.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System.Reflection;
using System.Threading.Tasks;
using Acr.Settings;
using FormsSkiaBikeTracker.Services;
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross.Core.ViewModels;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using MvvmCross.Plugins.JsonLocalization;
using FormsSkiaBikeTracker.ViewModels;
using LRPLib.Mvx.Core;
using LRPLib.Mvx.Services.Localization;
using LRPLib.Services;
using LRPLib.Services.Resources;
using SimpleCrypto;

namespace FormsSkiaBikeTracker
{
    public class App : MvxApplication
    {
        private LrpAppStart<LoadingViewModel> _AppStart { get; set; }

        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            InitializeResources();
            InitializeText();
            InitializeServices();

            _AppStart = new LrpAppStart<LoadingViewModel>(new { textTypeKey = Constants.TextTypeKey });
            _AppStart.Started += AppStartCompleted;
            RegisterAppStart(_AppStart);
        }

        private void AppStartCompleted()
        {
            InitializeBootstrap();
        }

        public static void InitializeResources()
        {
            Assembly currentAssembly = Assembly.Load(new AssemblyName("FormsSkiaBikeTracker.Shared"));
            ResourceLocator resourceLocator = new ResourceLocator(Constants.GeneralNamespace, currentAssembly);

            resourceLocator.RegisterPath(ResourceKeys.RootKey, Constants.RootResourcesFolder);
            resourceLocator.RegisterPath(ResourceKeys.ImagesKey, Constants.RootImagesFolder);
            resourceLocator.RegisterPath(ResourceKeys.TextKey, Constants.RootTextFolder);
            resourceLocator.RegisterPath("Fonts", Constants.RootResourcesFolder + "/Fonts");

            Mvx.RegisterSingleton<ISettings>(Settings.Current);
            Mvx.RegisterSingleton<IResourceLocator>(resourceLocator);
        }

        private void InitializeText()
        {
            LrpTextProviderBuilder builder = new LrpTextProviderBuilder(Constants.RootTextFolder, Constants.TextTypeKey);

            Mvx.RegisterSingleton<IMvxTextProviderBuilder>(builder);
            Mvx.RegisterSingleton<IMvxTextProvider>(builder.TextProvider);

            // Set language
#if DEBUG
            builder.LoadResources("fr");
#else
            builder.LoadResources(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
#endif
        }

        private void InitializeServices()
        {
            Mvx.RegisterSingleton(Mvx.IocConstruct<LrpBootstrapper>);
            Mvx.RegisterSingleton<ICryptoService>(() => new PBKDF2());
            Mvx.RegisterSingleton<ILocationTracker>(InitializeLocationTracker);
        }

        private ILocationTracker InitializeLocationTracker()
        {
            ILocationTracker tracker = Mvx.IocConstruct<LocationTracker>();

            tracker.Start(8, 3, false);

            return tracker;
        }

        private void InitializeBootstrap()
        {
            LrpBootstrapper bootstrapper = Mvx.Resolve<LrpBootstrapper>();

            bootstrapper.AddAsyncStep(new LrpAsyncActionBootstrapStep(a => Task.Delay(1500).Wait()));
            bootstrapper.QueueStep(new LrpActionBootstrapStep(() => { }) { StepActionText = "Booting..." });

            bootstrapper.Boot();
        }
    }
}