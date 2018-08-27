using System;
using System.Collections.Specialized;

namespace Xamarin.Forms.Maps.Overlays.WeakSubscription
{
    internal class WeakCollectionChangedSubscription : WeakEventSubscription<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>
    {
        public WeakCollectionChangedSubscription(INotifyCollectionChanged source, EventHandler<NotifyCollectionChangedEventArgs> targetHandler)
            : base(source, nameof(INotifyCollectionChanged.CollectionChanged), targetHandler)
        {
        }

        protected override Delegate CreateEventHandler()
        {
            return new NotifyCollectionChangedEventHandler(OnSourceEvent);
        }
    }
}
