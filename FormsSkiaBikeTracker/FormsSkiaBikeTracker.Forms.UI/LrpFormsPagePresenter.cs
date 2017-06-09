// **********************************************************************
// 
//   LrpFormsPagePresenter.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Threading.Tasks;
using LRPLib.Services;
using MvvmCross.Core.ViewModels;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Platform;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI
{
    public abstract class LrpFormsPagePresenter : MvxFormsPagePresenter
    {
        protected LrpFormsPagePresenter()
        {
        }

        protected LrpFormsPagePresenter(Application mvxFormsApp) : base(mvxFormsApp)
        {
        }

        public override void Show(MvxViewModelRequest request)
        {
            Page newPage = CreateAndSetupPage(request);

            if (newPage != null)
            {
                NavigationPage mainPage = MvxFormsApp.MainPage as NavigationPage;
                bool replaceMain = false;
                bool wrapInNavPage = false;
                bool setAsNavigationRoot = false;

                if (request.PresentationValues != null)
                {
                    if (request.PresentationValues.ContainsKey(PresenterConstants.ReplaceMainPage))
                    {
                        bool.TryParse(request.PresentationValues[PresenterConstants.ReplaceMainPage], out replaceMain);
                    }
                    if (request.PresentationValues.ContainsKey(PresenterConstants.WrapWithNavigationPage))
                    {
                        bool.TryParse(request.PresentationValues[PresenterConstants.WrapWithNavigationPage], out wrapInNavPage);
                    }
                    if (request.PresentationValues.ContainsKey(PresenterConstants.SetAsNavigationRoot))
                    {
                        bool.TryParse(request.PresentationValues[PresenterConstants.SetAsNavigationRoot], out setAsNavigationRoot);
                    }
                }

                if (wrapInNavPage)
                {
                    newPage = new NavigationPage(newPage);
                }

                if (replaceMain || mainPage == null)
                {
                    SetMainPage(newPage);
                }
                else
                {
                    try
                    {
                        Task pushTask = mainPage.PushAsync(newPage);

                        if (setAsNavigationRoot)
                        {
                            NavigationPage.SetHasBackButton(newPage, false);

                            MainThread.RunAsync(async () =>
                                                {
                                                    await pushTask.ConfigureAwait(true);

                                                    ClearNavigationStack(mainPage);
                                                });
                        }
                    }
                    catch (Exception e)
                    {
                        Mvx.Error("Exception pushing {0}: {1}\n{2}", newPage.GetType(), e.Message, e.StackTrace);
                    }
                }
            }
        }

        private void SetMainPage(Page newMain)
        {
            MvxFormsApp.MainPage = newMain;
            InitRootViewController(newMain);
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

        protected override void CustomPlatformInitialization(NavigationPage mainPage)
        {
            InitRootViewController(mainPage);
        }

        protected abstract void InitRootViewController(Page rootPage);

        private void ClearNavigationStack(NavigationPage navPage)
        {
            while (navPage.Navigation.NavigationStack.Count > 1)
            {
                navPage.Navigation.RemovePage(navPage.Navigation.NavigationStack[0]);
            }
        }
    }
}
