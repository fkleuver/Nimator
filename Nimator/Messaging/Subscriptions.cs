using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nimator.Util;

namespace Nimator.Messaging
{
    internal static class Subscriptions
    {
        // ReSharper disable InconsistentNaming | static properties in a static class should have the same conventions as instance properties in a normal class
        private static readonly object _subscriptionsLock = new object();
        private static readonly List<Subscription> _subscriptions = new List<Subscription>();
        // ReSharper enable InconsistentNaming
        private static int _subscriptionsChangeCounter;

        [ThreadStatic]
        private static int _localSubscriptionRevision;

        [ThreadStatic]
        private static Subscription[] _localSubscriptions;
        
        internal static Guid Register<T>(Action<T> action)
        {
            var type = typeof(T);
            var key = GuidGenerator.GenerateTimeBasedGuid();
            var subscription = new Subscription(type, key, action);

            lock (_subscriptionsLock)
            {
                _subscriptions.Add(subscription);
                _subscriptionsChangeCounter++;
            }

            return key;
        }

        internal static void Unregister(Guid token)
        {
            lock (_subscriptionsLock)
            {
                var subscription = _subscriptions.Find(s => s.Token == token);
                if (subscription == null)
                {
                    return;
                }

                var removed = _subscriptions.Remove(subscription);
                if (!removed)
                {
                    return;
                }

                if (_localSubscriptions != null)
                {
                    var localIdx = Array.IndexOf(_localSubscriptions, subscription);
                    if (localIdx >= 0)
                    {
                        _localSubscriptions = RemoveAt(_localSubscriptions, localIdx);
                    }
                }

                _subscriptionsChangeCounter++;
            }
        }

        internal static void Clear()
        {
            lock (_subscriptionsLock)
            {
                _subscriptions.Clear();
                if (_localSubscriptions != null)
                {
                    Array.Clear(_localSubscriptions, 0, _localSubscriptions.Length);
                }
                _subscriptionsChangeCounter++;
            }
        }

        internal static bool IsRegistered(Guid token)
        {
            lock (_subscriptionsLock)
            {
                return _subscriptions.Any(s => s.Token == token);
            }
        }

        internal static Subscription[] GetSubscriptions()
        {
            if (_localSubscriptions == null)
            {
                _localSubscriptions = new Subscription[0];
            }

            var changeCounterLatestCopy = Interlocked.CompareExchange(ref _subscriptionsChangeCounter, 0, 0);
            if (_localSubscriptionRevision == changeCounterLatestCopy)
            {
                return _localSubscriptions;
            }

            Subscription[] latestSubscriptions;
            lock (_subscriptionsLock)
            {
                latestSubscriptions = _subscriptions.ToArray();
            }

            _localSubscriptionRevision = changeCounterLatestCopy;
            _localSubscriptions = latestSubscriptions;
            return _localSubscriptions;
        }

        private static T[] RemoveAt<T>(T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
            {
                Array.Copy(source, 0, dest, 0, index);
            }

            if (index < source.Length - 1)
            {
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
            }

            return dest;
        }
    }
}
