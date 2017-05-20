using FormsSkiaBikeTracker.Forms.UI;
using MvvmCross.Droid.Views;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Droid
{
    public class LrpFormsAndroidPagePresenter : LrpFormsPagePresenter, IMvxAndroidViewPresenter
    {
        public LrpFormsAndroidPagePresenter() : base()
        {

        }

        public LrpFormsAndroidPagePresenter(Application mvxFormsApp) : base(mvxFormsApp)
        {
        }

        protected override void PlatformRootViewInitialization(Page rootPage)
        {
        }
    }
}
