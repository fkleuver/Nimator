namespace Nimator.Formatters
{
    /// <summary>
    /// An output formatter that transforms a <see cref="HealthCheckResult"/> into a readable string.
    /// </summary>
    public interface IHealthCheckResultFormatter
    {
        /// <summary>
        /// Transforms the provided <see cref="HealthCheckResult"/> into a readable string.
        /// </summary>
        /// <param name="result">The <see cref="HealthCheckResult"/> to transform.</param>
        /// <returns>A string representation of the provided <see cref="HealthCheckResult"/>.</returns>
        string Format([NotNull]HealthCheckResult result);
    }
}
