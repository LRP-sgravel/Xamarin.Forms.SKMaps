// **********************************************************************
// 
//   LoadingViewModel.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using LRPLib.Mvx.ViewModels;
using LRPLib.Services;
using MvvmCross.Core.ViewModels;
using Realms;

namespace FormsSkiaBikeTracker.ViewModels
{
    class LoadingViewModel : LrpViewModel
    {
        private LrpBootstrapper _Bootstrapper { get; }

        private string _statusTextId;
        private string _StatusTextId
        {
            get { return _statusTextId; }
            set
            {
                if (_StatusTextId != value)
                {
                    _statusTextId = value;
                    RaisePropertyChanged(() => StatusText);
                }
            }
        }

        public string StatusText
        {
            get { return string.IsNullOrEmpty(_StatusTextId) ? _StatusTextId : LanguageBinder.GetText(_StatusTextId); }
        }

        public LoadingViewModel(LrpBootstrapper bootstrapper) : base(Constants.TextTypeKey, false)
        {
            _Bootstrapper = bootstrapper;
        }

        public override void Start()
        {
            base.Start();

            _Bootstrapper.BootTextChanged += UpdateBootText;
            _Bootstrapper.BootCompleted += OnBootCompleted;

            if (_Bootstrapper.IsBooted)
            {
                MainThread.Run(OnBootCompleted);
            }
        }

        private void UpdateBootText(string status)
        {
            _StatusTextId = status;
        }

        public void OnBootCompleted()
        {
            MvxBundle presentationBundle = new MvxBundle(new Dictionary<string, string>
                                                            {
                                                                [PresenterConstants.WrapWithNavigationPage] =
                                                                true.ToString()
                                                            });

            _Bootstrapper.BootTextChanged -= UpdateBootText;
            _Bootstrapper.BootCompleted -= OnBootCompleted;

            if (Realm.GetInstance()
                     .All<Athlete>()
                     .Any())
            {

                ShowViewModel<LoginViewModel>((IMvxBundle)null, presentationBundle);
            }
            else
            {
                ShowViewModel<SignUpViewModel>(new { signInOnCompletion = true }, presentationBundle);
            }
        }
    }
}
