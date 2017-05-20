using MvvmCross.Core.ViewModels;
using MvvmCross.Forms.Presenter.Core;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI
{
    public abstract class LrpFormsPagePresenter : MvxFormsPagePresenter
    {
        public const string ReplaceMainPagePresentation = nameof(ReplaceMainPagePresentation);

        protected LrpFormsPagePresenter()
        {
        }

        protected LrpFormsPagePresenter(Application mvxFormsApp) : base(mvxFormsApp)
        {
        }

        public override void Show(MvxViewModelRequest request)
        {
            bool callBase = true;
            bool replaceMain = false;

            if (request.PresentationValues != null)
            {
                if (request.PresentationValues.ContainsKey(ReplaceMainPagePresentation))
                {
                    bool.TryParse(request.PresentationValues[ReplaceMainPagePresentation], out replaceMain);
                }
            }

            if (replaceMain)
            {
                Page newMain = CreateAndSetupPage(request);

                if (newMain != null)
                {
                    MvxFormsApp.MainPage = newMain;
                    callBase = false;
                }
            }
            
            if (callBase)
            {
                base.Show(request);
            }
        }

        private Page CreateAndSetupPage(MvxViewModelRequest request)
        {
            Page page = MvxPresenterHelpers.CreatePage(request);

            if (page != null)
            {
                IMvxViewModel viewModel = MvxPresenterHelpers.LoadViewModel(request);

                SetupForBinding(page, viewModel, request);
            }

            return page;
        }

        private void SetupForBinding(Page page, IMvxViewModel viewModel, MvxViewModelRequest request)
        {
            IMvxContentPage mvxContentPage = page as IMvxContentPage;

            if (mvxContentPage != null)
            {
                mvxContentPage.Request = request;
                mvxContentPage.ViewModel = viewModel;
            }
            else
            {
                page.BindingContext = viewModel;
            }
        }

        protected abstract void PlatformRootViewInitialization(Page rootPage);
    }
}
