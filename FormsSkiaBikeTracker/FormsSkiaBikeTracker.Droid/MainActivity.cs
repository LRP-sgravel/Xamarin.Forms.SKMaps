using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using MvvmCross.Binding.BindingContext;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Platform;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Presenter.Droid;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Views;

namespace FormsSkiaBikeTracker.Droid
{
    [Activity(Label = "FormsSkiaBikeTracker",
              Theme = "@style/MainTheme",
              ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : FormsAppCompatActivity, IMvxAndroidView
    {
        public IMvxBindingContext BindingContext { get; set; }
        public static bool IsAppInForeground { get; private set; }
        public static bool IsAppInBg { get; private set; }
        private IMvxAndroidActivityLifetimeListener _LifetimeListener => Mvx.Resolve<IMvxAndroidActivityLifetimeListener>();

        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        public IMvxViewModel ViewModel
        {
            get { return DataContext as IMvxViewModel; }
            set
            {
                DataContext = value;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            // Set the layout resources first
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate(bundle);

            // Required for proper Push notifications handling
            var setupSingleton = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setupSingleton.EnsureInitialized();

            Xamarin.Forms.Forms.Init(this, bundle);

            MvxFormsApp mvxFormsApp = new MvxFormsApp();
            LoadApplication(mvxFormsApp);

            MvxFormsDroidPagePresenter presenter = Mvx.Resolve<IMvxViewPresenter>() as MvxFormsDroidPagePresenter;
            presenter.MvxFormsApp = mvxFormsApp;

            IsAppInForeground = true;

            _LifetimeListener.OnCreate(this);

            Mvx.Resolve<IMvxAppStart>()
               .Start();
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            StartActivityForResult(intent, requestCode);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _LifetimeListener.OnStart(this);

            if (IsAppInBg)
            {
                IsAppInBg = false;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            _LifetimeListener.OnStop(this);

            if (!IsAppInForeground)
            {
                IsAppInBg = true;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            _LifetimeListener.OnResume(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _LifetimeListener.OnDestroy(this);
        }

        protected override void OnPause()
        {
            base.OnPause();

            _LifetimeListener.OnPause(this);
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            _LifetimeListener.OnRestart(this);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            IsAppInForeground = hasFocus;
        }
    }
}