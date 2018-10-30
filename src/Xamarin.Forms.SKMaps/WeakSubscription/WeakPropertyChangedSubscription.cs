using System;
using System.ComponentModel;

namespace Xamarin.Forms.SKMaps.WeakSubscription
{
    internal class WeakPropertyChangedSubscription : WeakEventSubscription<INotifyPropertyChanged, PropertyChangedEventArgs>
    {
        private string _propertyName;

        public WeakPropertyChangedSubscription(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> targetHandler)
            : this(source, null, targetHandler)
        {
        }

        public WeakPropertyChangedSubscription(INotifyPropertyChanged source, string propertyName, EventHandler<PropertyChangedEventArgs> targetHandler)
            : base(source, nameof(INotifyPropertyChanged.PropertyChanged), targetHandler)
        {
            _propertyName = propertyName;
        }

        protected override Delegate CreateEventHandler()
        {
            return new PropertyChangedEventHandler(OnPropertyChangedEvent);
        }

        private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(_propertyName) ||
                _propertyName == args.PropertyName)
            {
                OnSourceEvent(sender, args);
            }
        }
    }
}
