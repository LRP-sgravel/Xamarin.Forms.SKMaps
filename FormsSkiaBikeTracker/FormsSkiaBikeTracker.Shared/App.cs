using System.Reflection;
using System.Threading.Tasks;
using Acr.Settings;
using MvvmCross.Core.ViewModels;
using MvvmCross.Localization;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using MvvmCross.Plugins.JsonLocalization;
using FormsSkiaBikeTracker.ViewModels;
using LRPLib.Mvx.Core;
using LRPLib.Mvx.Services.Localization;
using LRPLib.Services;

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

            resourceLocator.RegisterPath(ResourceLocator.RootKey, Constants.RootResourcesFolder);
            resourceLocator.RegisterPath(ResourceLocator.ImagesKey, Constants.RootImagesFolder);
            resourceLocator.RegisterPath(ResourceLocator.TextKey, Constants.RootTextFolder);

            Mvx.RegisterSingleton<ISettings>(Settings.Current);
            Mvx.RegisterSingleton(resourceLocator);
        }

        private void InitializeText()
        {
            LrpTextProviderBuilder builder = new LrpTextProviderBuilder(Constants.RootTextFolder, Constants.TextTypeKey);

            Mvx.RegisterSingleton<IMvxTextProviderBuilder>(builder);
            Mvx.RegisterSingleton<IMvxTextProvider>(builder.TextProvider);

            // Set language
            builder.LoadResources("fr");
        }

        private void InitializeServices()
        {
            Mvx.RegisterSingleton(Mvx.IocConstruct<LrpBootstrapper>);
        }

        private void InitializeBootstrap()
        {
            LrpBootstrapper bootstrapper = Mvx.Resolve<LrpBootstrapper>();

            bootstrapper.AddAsyncStep(new LrpAsyncActionBootstrapStep(async a => await Task.Delay(1500).ConfigureAwait(false)));
            bootstrapper.QueueStep(new LrpActionBootstrapStep(() => { }) { StepActionText = "Booting..." });

            bootstrapper.Boot();
        }
    }
}