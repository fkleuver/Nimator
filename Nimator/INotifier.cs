namespace Nimator
{
    /// <summary>
    /// Represents an outgoing notification channel for instances of <see cref="HealthCheckResult"/>.
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// Sends out a <see cref="HealthCheckResult"/> via this notification channel.
        /// </summary>
        void Send([NotNull]HealthCheckResult result);
    }
}
