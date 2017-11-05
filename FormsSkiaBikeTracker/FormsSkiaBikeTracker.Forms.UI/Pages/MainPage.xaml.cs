// **********************************************************************
// 
//   MainPage.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            MapControl.MoveToRegion(new MapSpan(new Position(0, 0), 0.5, 0.5));
        }
    }
}
