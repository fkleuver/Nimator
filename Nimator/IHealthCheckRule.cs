﻿namespace Nimator
{
    /// <summary>
    /// Represents a business rule that creates a <see cref="HealthCheckResult"/> based
    /// on arbitrary data.
    /// </summary>
    public interface IHealthCheckRule
    {
        /// <summary>
        /// Whether a warning event should be created if this rule is not matched against any of the collected data.
        /// </summary>
        bool WarnIfNotMatched { get; }

        /// <summary>
        /// Determine whether provided data should be processed by this rule.
        /// </summary>
        /// <param name="value">The data to test.</param>
        /// <returns>True if this rule should process the data, otherwise false.</returns>
        bool IsMatch([NotNull]object value);

        /// <summary>
        /// Creates a <see cref="HealthCheckResult"/> based on the provided data.
        /// </summary>
        /// <param name="value">The data on which the <see cref="HealthCheckResult"/> is applicable.</param>
        /// <returns>A <see cref="HealthCheckResult"/> representing the outcome of this rule.</returns>
        HealthCheckResult GetResult([NotNull]object value);
    }
}
