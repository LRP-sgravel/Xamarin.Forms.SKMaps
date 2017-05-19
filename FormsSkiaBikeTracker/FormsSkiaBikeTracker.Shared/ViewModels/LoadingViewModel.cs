using LRPLib.Mvx.ViewModels;
using LRPLib.Services;

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

            _Bootstrapper.BootTextChanged += status => { _StatusTextId = status; };
            _Bootstrapper.BootCompleted += OnBootCompleted;

            if (_Bootstrapper.IsBooted)
            {
                MainThread.Context.Post(state => OnBootCompleted(), null);
            }
        }

        public void OnBootCompleted()
        {

        }
    }
}
