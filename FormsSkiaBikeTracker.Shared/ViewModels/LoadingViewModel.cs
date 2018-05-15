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

using System;
using System.Linq;
using System.Threading.Tasks;
using FormsSkiaBikeTracker.Models;
using FormsSkiaBikeTracker.Shared.ViewModels;
using LRPFramework.Mvx.ViewModels;
using LRPFramework.Services;
using Realms;

namespace FormsSkiaBikeTracker.ViewModels
{
    class LoadingViewModel : LRPViewModel
    {
        private ILRPBootstrapper _Bootstrapper { get; }

        private string _statusTextId;
        private string _StatusTextId
        {
            get => _statusTextId;
            set
            {
                if (_StatusTextId != value)
                {
                    _statusTextId = value;
                    RaisePropertyChanged(() => StatusText);
                }
            }
        }

        public string StatusText => string.IsNullOrEmpty(_StatusTextId) ? _StatusTextId : LanguageBinder.GetText(_StatusTextId);

        public LoadingViewModel(ILRPBootstrapper bootstrapper)
        {
            _Bootstrapper = bootstrapper;
        }

        public override void Prepare()
        {
            base.Prepare();

            TextTypeKey = Constants.TextTypeKey;
        }

        public override Task Initialize()
        {
            _Bootstrapper.BootTextChanged += UpdateBootText;
            _Bootstrapper.BootCompleted += OnBootCompleted;

            return base.Initialize();
        }

        private void UpdateBootText(object sender, string status)
        {
            _StatusTextId = status;
        }

        public void OnBootCompleted(object sender, EventArgs args)
        {
            _Bootstrapper.BootTextChanged -= UpdateBootText;
            _Bootstrapper.BootCompleted -= OnBootCompleted;

            if (Realm.GetInstance()
                     .All<Athlete>()
                     .Any())
            {
                NavigationService.Navigate<LoginViewModel>();
            }
            else
            {
                NavigationService.Navigate<SignUpViewModel, bool>(true);
            }
        }
    }
}
