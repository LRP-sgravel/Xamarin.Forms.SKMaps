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
using Acr.Settings;
using Acr.UserDialogs;
using Xamarin.Forms.SKMaps.Sample.Services;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using MvvmCross.Localization;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.JsonLocalization;
using MvvmCross.ViewModels;
using SimpleCrypto;
using System.Globalization;
using Xamarin.Forms.SKMaps.Sample.ViewModels;
using Xamarin.Forms.SKMaps.Sample.Models;
using Realms;
using System.Linq;
using MvvmCross.Base;

namespace Xamarin.Forms.SKMaps.Sample
{
    public class MvxApp : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            InitializeServices();

            if (Realm.GetInstance(RealmConstants.RealmConfiguration)
                     .All<Athlete>()
                     .Any())
            {
                RegisterAppStart<LoginViewModel>();
            }
            else
            {
                RegisterAppStart<SignUpViewModel>();
            }

            base.Initialize();
        }

        private void InitializeServices()
        {
            Mvx.LazyConstructAndRegisterSingleton<ICryptoService, PBKDF2>();
            Mvx.RegisterSingleton<ILocationTracker>(InitializeLocationTracker);
            Mvx.RegisterSingleton<IResourceLocator>(InitializeResources);
            Mvx.RegisterSingleton<IUserDialogs>(UserDialogs.Instance);
            Mvx.RegisterSingleton<ISettings>(CrossSettings.Current);
            Mvx.RegisterType<IRouteRecorder, RouteRecorder>();

            Mvx.CallbackWhenRegistered<IMvxMainThreadAsyncDispatcher>(InitializeText);
        }

        private ILocationTracker InitializeLocationTracker()
        {
            ILocationTracker tracker = Mvx.IocConstruct<LocationTracker>();

            tracker.Start(3, 2, false);

            return tracker;
        }
        
        public static IResourceLocator InitializeResources()
        {
            Assembly currentAssembly = Assembly.GetAssembly(typeof(MvxApp));
            ResourceLocator resourceLocator = new ResourceLocator(Constants.GeneralNamespace, currentAssembly);

            resourceLocator.RegisterPath(ResourceKeys.RootKey, Constants.RootResourcesFolder);
            resourceLocator.RegisterPath(ResourceKeys.ImagesKey, Constants.RootImagesFolder);
            resourceLocator.RegisterPath(ResourceKeys.TextKey, Constants.RootTextFolder);
            resourceLocator.RegisterPath("Fonts", Constants.RootResourcesFolder + "/Fonts");

            return resourceLocator;
        }

        private void InitializeText()
        {
            TextProviderBuilder builder = new TextProviderBuilder(Constants.RootTextFolder, Constants.DefaultTextTypeKey);

            Mvx.RegisterSingleton<IMvxTextProviderBuilder>(builder);
            Mvx.RegisterSingleton<IMvxTextProvider>(builder.TextProvider);
        }
    }
}