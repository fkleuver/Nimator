using System;
using System.Diagnostics;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Rules
{
    /// <inheritdoc />
    /// <summary>
    /// Will match and handle <see cref="T:Nimator.DataCollectionResult" /> with any exception and flag it to stop being processed by other rules.
    /// </summary>
    public sealed class DataCollectorError : IHealthCheckRule
    {
        public bool WarnIfNotMatched { get; } = false;
        public Identity CheckId { get; }
        
        public DataCollectorError([NotNull]string name) : this(new Identity(Guard.AgainstNullAndEmpty_Return(nameof(name), name))) { }
        public DataCollectorError([NotNull]Identity checkId)
        {
            Guard.AgainstNull(nameof(checkId), checkId);

            CheckId = checkId;
        }


        /// <inheritdoc />
        public bool IsMatch([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            return value is IDataCollectionResult result && result.Error != null;
        }
        
        /// <inheritdoc />
        public HealthCheckResult GetResult([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            if (!IsMatch(value))
            {
                throw new ArgumentException($"{nameof(DataCollectorError)} received a data object that does not contain an exception.");
            }

            var dataResult = value as IDataCollectionResult;
            Debug.Assert(dataResult != null, nameof(dataResult) + " != null");

            var healthResult = HealthCheckResult
                .Create(CheckId)
                .SetLevel(LogLevel.Fatal)
                .SetErrorMessage(dataResult.Error.Message)
                .SetException(dataResult.Error);
            
            if (dataResult.Error.GetType() == typeof(TimeoutException))
            {
                healthResult
                    .SetStatus(Status.Critical)
                    .SetReason($"The request to collect data from \"{dataResult.Origin.Id.Name}\" for \"{CheckId.Name}\" timed out.");
            }
            else
            {
                healthResult
                    .SetStatus(Status.Unknown)
                    .SetReason($"Nimator failed while trying to collect data from \"{dataResult.Origin.Id.Name}\" for \"{CheckId.Name}\".");
            }

            dataResult.StopProcessing();

            return healthResult;
        }
    }
}
