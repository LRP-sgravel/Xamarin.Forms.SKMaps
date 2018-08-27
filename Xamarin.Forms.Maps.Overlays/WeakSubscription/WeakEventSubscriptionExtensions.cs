using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Xamarin.Forms.Maps.Overlays.WeakSubscription
{
    internal static class WeakEventSubscriptionExtensions
    {
        public static WeakEventSubscription<TSource> WeakSubscribe<TSource>(this TSource source, string sourceEventName, EventHandler target)
            where TSource : class
        {
            WeakEventSubscription<TSource> subscription = new WeakEventSubscription<TSource>(source, sourceEventName, target);

            subscription.RegisterEvent();

            return subscription;
        }

        public static WeakEventSubscription<TSource, TEventArgs> WeakSubscribe<TSource, TEventArgs>(this TSource source, string sourceEventName, EventHandler<TEventArgs> target)
            where TSource : class
        {
            WeakEventSubscription<TSource, TEventArgs> subscription = new WeakEventSubscription<TSource, TEventArgs>(source, sourceEventName, target);

            subscription.RegisterEvent();

            return subscription;
        }

        public static WeakPropertyChangedSubscription WeakSubscribe(this INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> target)
        {
            return source.WeakSubscribe(null, target);
        }

        public static WeakPropertyChangedSubscription WeakSubscribe<T>(this INotifyPropertyChanged source, Expression<Func<T>> property, EventHandler<PropertyChangedEventArgs> target)
        {
            return source.WeakSubscribe((property.Body as MemberExpression).Member.Name, target);
        }

        public static WeakPropertyChangedSubscription WeakSubscribe(this INotifyPropertyChanged source, string propertyName, EventHandler<PropertyChangedEventArgs> target)
        {
            WeakPropertyChangedSubscription subscription = new WeakPropertyChangedSubscription(source, propertyName, target);

            subscription.RegisterEvent();

            return subscription;
        }

        public static WeakCollectionChangedSubscription WeakSubscribe(this INotifyCollectionChanged source, EventHandler<NotifyCollectionChangedEventArgs> target)
        {
            WeakCollectionChangedSubscription subscription = new WeakCollectionChangedSubscription(source, target);

            subscription.RegisterEvent();

            return subscription;
        }
    }
}
