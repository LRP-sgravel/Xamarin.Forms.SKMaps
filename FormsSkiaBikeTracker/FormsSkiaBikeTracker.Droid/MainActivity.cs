// **********************************************************************
// 
//   MainActivity.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using LRPLib;
using LRPLib.Mvx.Droid;
using LRPLib.Mvx.Views.XForms.Droid;
using LRPLib.Views.XForms.Droid;
using MvvmCross.Binding.BindingContext;
using Xamarin.Forms.Platform.Android;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Platform;
using MvvmCross.Core.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Views;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Droid.Views;

namespace FormsSkiaBikeTracker.Droid
{
    [Activity(Label = "FormsSkiaBikeTracker",
              Theme = "@style/MainTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : FormsAppCompatActivity, IMvxAndroidView, IMvxEventSourceActivity
    {
        public event EventHandler DisposeCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> CreateWillBeCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> CreateCalled;
        public event EventHandler DestroyCalled;
        public event EventHandler<MvxValueEventArgs<Intent>> NewIntentCalled;
        public event EventHandler ResumeCalled;
        public event EventHandler PauseCalled;
        public event EventHandler StartCalled;
        public event EventHandler RestartCalled;
        public event EventHandler StopCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> SaveInstanceStateCalled;
        public event EventHandler<MvxValueEventArgs<MvxStartActivityForResultParameters>> StartActivityForResultCalled;
        public event EventHandler<MvxValueEventArgs<MvxActivityResultParameters>> ActivityResultCalled;

        public IMvxBindingContext BindingContext { get; set; }
        public static bool IsAppInForeground { get; private set; }
        public static bool IsAppInBg { get; private set; }
        private IMvxAndroidActivityLifetimeListener _LifetimeListener => Mvx.Resolve<IMvxAndroidActivityLifetimeListener>();

        public object DataContext
        {
            get { return BindingContext?.DataContext; }
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeCalled?.Raise(this);
        }

        protected override void OnCreate(Bundle bundle)
        {
            // Set the layout resources first
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            CreateWillBeCalled?.Raise(this, bundle);
            base.OnCreate(bundle);
            CreateCalled?.Raise(this, bundle);

            // Required for proper Push notifications handling
            var setupSingleton = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
            setupSingleton.EnsureInitialized();

            Xamarin.Forms.Forms.Init(this, bundle);

            MvxFormsApp mvxFormsApp = new MvxFormsApp();
            LoadApplication(mvxFormsApp);
            
            LrpLib.Init();
            LrpLibMvx.Init();
            LrpLibViews.Init();
            LrpLibMvxViews.Init();

            LrpFormsAndroidPagePresenter presenter = Mvx.Resolve<IMvxViewPresenter>() as LrpFormsAndroidPagePresenter;
            presenter.MvxFormsApp = mvxFormsApp;

            IsAppInForeground = true;

            this.AddEventListeners();
            _LifetimeListener.OnCreate(this);

            Mvx.Resolve<IMvxAppStart>()
               .Start();
        }

        protected override void OnStart()
        {
            base.OnStart();
            StartCalled?.Raise(this);

            _LifetimeListener.OnStart(this);

            if (IsAppInBg)
            {
                IsAppInBg = false;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopCalled?.Raise(this);

            _LifetimeListener.OnStop(this);

            if (!IsAppInForeground)
            {
                IsAppInBg = true;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            ResumeCalled?.Raise(this);

            _LifetimeListener.OnResume(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyCalled?.Raise(this);

            _LifetimeListener.OnDestroy(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            PauseCalled?.Raise(this);

            _LifetimeListener.OnPause(this);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            RestartCalled?.Raise(this);

            _LifetimeListener.OnRestart(this);
        }
        
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            NewIntentCalled?.Raise(this, intent);
        }
        
        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            base.StartActivityForResult(intent, requestCode);
            StartActivityForResultCalled?.Raise(this, new MvxStartActivityForResultParameters(intent, requestCode));
        }

        public override void StartActivityForResult(Intent intent, int requestCode)
        {
            base.StartActivityForResult(intent, requestCode);
            StartActivityForResultCalled?.Raise(this, new MvxStartActivityForResultParameters(intent, requestCode));
        }

        public override void StartActivityForResult(Intent intent, int requestCode, Bundle options)
        {
            base.StartActivityForResult(intent, requestCode, options);
            StartActivityForResultCalled?.Raise(this, new MvxStartActivityForResultParameters(intent, requestCode));
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            ActivityResultCalled?.Raise(this, new MvxActivityResultParameters(requestCode, resultCode, data));
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            SaveInstanceStateCalled?.Raise(this, outState);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            IsAppInForeground = hasFocus;
        }
    }
}