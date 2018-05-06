// **********************************************************************
// 
//   LrpFormsIosPagePresenter.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using FormsSkiaBikeTracker.Forms.UI;
using MvvmCross.iOS.Views.Presenters;
using UIKit;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.iOS
{
    public class LrpFormsIosPagePresenter : LRPFormsPagePresenter, IMvxIosViewPresenter
    {
        private UIWindow _Window { get; }

        public LrpFormsIosPagePresenter(UIWindow window, Xamarin.Forms.Application mvxFormsApp) : base(mvxFormsApp)
        {
            _Window = window;
        }

        protected override void InitRootViewController(Page mainPage)
        {
            _Window.RootViewController = mainPage.CreateViewController();
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
