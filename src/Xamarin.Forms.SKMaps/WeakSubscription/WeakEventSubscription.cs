using System;
using System.Reflection;

namespace Xamarin.Forms.SKMaps.WeakSubscription
{
    internal class WeakEventSubscription<TSource> : IDisposable where TSource : class
    {
        private WeakReference<TSource> _weakSource;
        private WeakReference _weakTarget;
        private EventInfo _sourceEvent;
        private MethodInfo _targetMethod;
        private Delegate _internalEventHandler;
        private bool _subscribed;

        public WeakEventSubscription(TSource source, string eventName, EventHandler targetHandler)
        {
            _weakSource = new WeakReference<TSource>(source);
            _sourceEvent = typeof(TSource).GetEvent(eventName);
            _weakTarget = new WeakReference(targetHandler.Target);
            _targetMethod = targetHandler.GetMethodInfo();
        }

        public void Dispose()
        {
            UnregisterEvent();
        }

        public void RegisterEvent()
        {
            if (_internalEventHandler == null)
            {
                _internalEventHandler = CreateEventHandler();
            }

            if (!_subscribed && _weakSource.TryGetTarget(out TSource sourceObject))
            {
                _sourceEvent.GetAddMethod().Invoke(sourceObject, new[] { _internalEventHandler });
                _subscribed = true;
            }
        }

        public void UnregisterEvent()
        {
            if (_subscribed && _weakSource.TryGetTarget(out TSource sourceObject))
            {
                _sourceEvent.GetRemoveMethod().Invoke(sourceObject, new[] { _internalEventHandler });
                _subscribed = false;
            }
        }

        private void OnSourceEvent(object sender, EventArgs args)
        {
            if (_weakTarget.IsAlive)
            {
                _targetMethod.Invoke(_weakTarget.Target, new[] { sender, args });
            }
            else
            {
                UnregisterEvent();
            }
        }

        private Delegate CreateEventHandler()
        {
            return new EventHandler(OnSourceEvent);
        }
    }

    internal class WeakEventSubscription<TSource, TEventArgs> : IDisposable where TSource : class
    {
        private WeakReference<TSource> _weakSource;
        private WeakReference _weakTarget;
        private EventInfo _sourceEvent;
        private MethodInfo _targetMethod;
        private Delegate _internalEventHandler;
        private bool _subscribed;

        public WeakEventSubscription(TSource source, string eventName, EventHandler<TEventArgs> targetHandler)
        {
            _weakSource = new WeakReference<TSource>(source);
            _sourceEvent = typeof(TSource).GetEvent(eventName);
            _weakTarget = new WeakReference(targetHandler.Target);
            _targetMethod = targetHandler.GetMethodInfo();
        }

        public void Dispose()
        {
            UnregisterEvent();
        }

        public void RegisterEvent()
        {
            if (_internalEventHandler == null)
            {
                _internalEventHandler = CreateEventHandler();
            }

            if (!_subscribed && _weakSource.TryGetTarget(out TSource sourceObject))
            {
                _sourceEvent.GetAddMethod().Invoke(sourceObject, new[] { _internalEventHandler });
                _subscribed = true;
            }
        }

        public void UnregisterEvent()
        {
            if (_subscribed && _weakSource.TryGetTarget(out TSource sourceObject))
            {
                _sourceEvent.GetRemoveMethod().Invoke(sourceObject, new[] { _internalEventHandler });
                _subscribed = false;
            }
        }

        protected void OnSourceEvent(object sender, TEventArgs args)
        {
            if (_weakTarget.IsAlive)
            {
                InvokeRegisteredEvent(sender, args);
            }
            else
            {
                UnregisterEvent();
            }
        }

        protected virtual Delegate CreateEventHandler()
        {
            return new EventHandler<TEventArgs>(OnSourceEvent);
        }

        protected void InvokeRegisteredEvent(object sender, TEventArgs args)
        {
            _targetMethod.Invoke(_weakTarget.Target, new[] { sender, args });
        }
    }
}
