using FormsSkiaBikeTracker.Forms.UI;
using MvvmCross.iOS.Views.Presenters;
using UIKit;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.iOS
{
    public class LrpFormsIosPagePresenter : LrpFormsPagePresenter, IMvxIosViewPresenter
    {
        private UIWindow _Window { get; }

        public LrpFormsIosPagePresenter(UIWindow window, Xamarin.Forms.Application mvxFormsApp) : base(mvxFormsApp)
        {
            _Window = window;
        }

        protected override void PlatformRootViewInitialization(Page rootPage)
        {
            base.PlatformRootViewInitialization(rootPage);

            _Window.RootViewController = MvxFormsApp.MainPage.CreateViewController();
        }

        public bool PresentModalViewController(UIViewController controller, bool animated)
        {
            _Window.RootViewController.PresentViewController(controller, animated, null);

            return true;
        }

        public void NativeModalViewControllerDisappearedOnItsOwn()
        {
        }
    }
}
