using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIKit;
using MvvmCross.Forms.Presenter.iOS;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Platform.Platform;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.Platform.IoC;
using FormsSkiaBikeTracker.Forms.UI.Views;
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

            MvxFormsApp xamarinFormsApp = new MvxFormsApp();

            return new MvxFormsIosPagePresenter(Window, xamarinFormsApp);
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
        }
    }
}