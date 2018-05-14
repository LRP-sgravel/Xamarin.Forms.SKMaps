// **********************************************************************
// 
//   LRPFormsDroidPagePresenter.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using FormsSkiaBikeTracker.Forms.UI;
using MvvmCross.Droid.Views;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Droid
{
    public class LRPFormsAndroidPagePresenter : LRPFormsPagePresenter, IMvxAndroidViewPresenter
    {
        public LRPFormsAndroidPagePresenter() : base()
        {
        }

        public LRPFormsAndroidPagePresenter(Application mvxFormsApp) : base(mvxFormsApp)
        {
        }

        protected override void InitRootViewController(Page rootPage)
        {
        }
    }
}
