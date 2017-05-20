using System.Collections.Generic;
using FormsSkiaBikeTracker.Forms.UI;
using LRPLib.Mvx.ViewModels;
using LRPLib.Services;
using MvvmCross.Core.ViewModels;

namespace FormsSkiaBikeTracker.ViewModels
{
    class LoadingViewModel : LrpLocalizedViewModel
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
                MainThread.Context.Post(state => OnBootCompleted(), null);
            }
        }

        private void UpdateBootText(string status)
        {
            _StatusTextId = status;
        }

        public void OnBootCompleted()
        {
            _Bootstrapper.BootTextChanged -= UpdateBootText;
            _Bootstrapper.BootCompleted -= OnBootCompleted;

            ShowViewModel<MainViewModel>((IMvxBundle)null,
                                         new MvxBundle
                                         (
                                             new Dictionary<string, string>
                                             {
                                                 [LrpFormsPagePresenter.ReplaceMainPagePresentation] =
                                                 true.ToString()
                                             }
                                         ));
        }
    }
}
