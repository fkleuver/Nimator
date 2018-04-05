using System;

namespace Nimator.Messaging
{
    internal sealed class Subscription
    {
        internal Subscription(Type type, Guid token, object handler)
        {
            Type = type;
            Token = token;
            Handler = handler;
        }

        internal void Handle<T>(T message)
        {
            var handler = Handler as Action<T>;
            // ReSharper disable once PossibleNullReferenceException | if this causes an exception, the EventAggregator will catch it
            handler(message);
        }

        internal Guid Token { get; }
        internal Type Type { get; }
        private object Handler { get; }
    }
}
