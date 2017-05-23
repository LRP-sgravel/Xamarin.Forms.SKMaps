// **********************************************************************
// 
//   LoadingPage.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Views
{
    public partial class LoadingPage
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
