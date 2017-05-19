using Android.App;
using MvvmCross.Droid.Views;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Droid
{
    [Activity(Label = "FormsSkiaBikeTracker",
              MainLauncher = true,
              Icon = "@drawable/icon",
              Theme = "@style/ThemeOverlay.AppCompat.Light",
              NoHistory = true)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen() : base(Resource.Layout.SplashScreen)
        {
        }

        private bool _isInitializationComplete = false;
        public override void InitializationComplete()
        {
            if (!_isInitializationComplete)
            {
                _isInitializationComplete = true;
                StartActivity(typeof(MainActivity));
            }
        }

        protected override void OnCreate(Android.OS.Bundle bundle)
        {
            Xamarin.Forms.Forms.Init(this, bundle);

            // Leverage controls' StyleId attrib. to Xamarin.UITest
            Xamarin.Forms.Forms.ViewInitialized += (object sender, ViewInitializedEventArgs e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.View.StyleId))
                {
                    e.NativeView.ContentDescription = e.View.StyleId;
                }
            };

            base.OnCreate(bundle);
        }
    }
}