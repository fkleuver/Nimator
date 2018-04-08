using System;
using Nimator.CouchBase.Util;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase.Rules
{
    /// <inheritdoc />
    /// <summary>
    /// Will match and handle <see cref="T:Nimator.DataCollectionResult{IResult}" /> with a non-success status and flag it to stop being processed by other rules.
    /// </summary>
    public sealed class ServiceUnresponsive : IHealthCheckRule
    {
        public bool WarnIfNotMatched { get; } = false;
        public Identity CheckId { get; }
        
        public ServiceUnresponsive([NotNull]string name) : this(new Identity(Guard.AgainstNullAndEmpty_Return(nameof(name), name))) { }
        public ServiceUnresponsive([NotNull]Identity checkId)
        {
            Guard.AgainstNull(nameof(checkId), checkId);

            CheckId = checkId;
        }

        /// <inheritdoc />
        public bool IsMatch([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            return value.IsIResult();
        }
        
        /// <inheritdoc />
        public HealthCheckResult GetResult([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            if (!IsMatch(value))
            {
                throw new ArgumentException($"{nameof(ServiceUnresponsive)} received a data object that does not contain an unsuccesful IResult.");
            }

            dynamic dataResult = value;

            if (dataResult.Error == null && !dataResult.Data.Success)
            {
                dataResult.StopProcessing();

                return HealthCheckResult
                    .Create(CheckId)
                    .SetStatus(Status.Critical)
                    .SetLevel(LogLevel.Error)
                    .SetReason($"Service did not respond to request from \"{dataResult.Origin.Id.Name}\".");
            }

            return null;
        }
    }
}
