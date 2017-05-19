using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using LRP_XFormsCore.Droid;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Views;
using MvvmCross.Forms.Presenter.Droid;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Platform.IoC;
using FormsSkiaBikeTracker.Forms.UI.Views;

namespace FormsSkiaBikeTracker.Droid
{
    public class Setup : MvxAndroidSetup
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
#if DEBUG
            return new LrpDebugTrace(true);
#else
            return new LrpDebugTrace();
#endif
        }

        protected override void InitializePlatformServices()
        {
            base.InitializePlatformServices();
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            MvxFormsDroidPagePresenter presenter = new MvxFormsDroidPagePresenter();
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
    }
}