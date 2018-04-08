using System;
using Nimator.Logging;
using Nimator.Util;

// ReSharper disable MemberCanBePrivate.Global

namespace Nimator.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// A synchronous implementation of the Event Aggregator pattern with thread-safe subscription management.
    /// </summary>
    public sealed class EventAggregator : IDisposable
    {
        private static ILog _logger;
        public static ILog Logger
        {
            set => _logger = value;
            private get => _logger ?? (_logger = LogProvider.GetCurrentClassLogger());
        }

        private EventAggregator() { }

        /// <summary>
        /// A global singleton instance of <see cref="EventAggregator"/>.
        /// </summary>
        public static EventAggregator Instance { get; } = new EventAggregator();


        /// <summary>
        /// Publishes the event to all subscribers.
        /// </summary>
        public void Publish<T>([CanBeNull]T @event)
        {
            Logger.Debug($"[{nameof(EventAggregator)}] Publishing event: {typeof(T).GetClosedGenericTypeName()}");
            var subscriptions = Subscriptions.GetSubscriptions();
            var msgType = typeof(T);

            // ReSharper disable once ForCanBeConvertedToForeach | Performance-critical code
            for (var idx = 0; idx < subscriptions.Length; idx++)
            {
                var subscription = subscriptions[idx];

                if (!subscription.Type.IsAssignableFrom(msgType))
                {
                    continue;
                }
                try
                {
                    subscription.Handle(@event);
                }
                catch (Exception e)
                {
                    Logger.ErrorException($"[{nameof(EventAggregator)}] An error occurred while invoking subscription with event: {typeof(T).GetClosedGenericTypeName()}", e);
                }
            }
        }
        
        /// <summary>
        /// Registers a callback to be invoked for events of type <see cref="T"/>
        /// </summary>
        public Guid Subscribe<T>([NotNull]Action<T> callback)
        {
            Guard.AgainstNull(nameof(callback), callback);
            return Subscriptions.Register(callback);
        }

        /// <summary>
        /// Removes the subscription represented by the specified <see cref="Guid"/>
        /// </summary>
        public void Unsubscribe(Guid subscriptionToken) => Subscriptions.Unregister(subscriptionToken);
        
        /// <summary>
        /// Checks if there is a subscription present represented by the specified <see cref="Guid"/>
        /// </summary>
        public bool IsSubscribed(Guid subscriptionToken) => Subscriptions.IsRegistered(subscriptionToken);

        /// <summary>
        /// Removes all subscriptions.
        /// </summary>
        public void ClearSubscriptions() => Subscriptions.Clear();

        /// <inheritdoc />
        /// <summary>
        /// Disposes this <see cref="T:Nimator.Messaging.EventAggregator" /> (removes all subscriptions).
        /// </summary>
        public void Dispose()
        {
            ClearSubscriptions();
        }
    }
}
