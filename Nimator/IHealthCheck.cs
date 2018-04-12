using System.Threading.Tasks;
using Nimator.Util;

namespace Nimator
{
    /// <summary>
    /// Represents an arbitrary health check operation that runs asynchronously.
    /// </summary>
    public interface IHealthCheck
    {
        /// <summary>
        /// The unique identifier for this operation.
        /// </summary>
        [NotNull]
        Identity Id { get; }

        bool NeedsToRun { get; }

        /// <summary>
        /// Runs this task asynchronously.
        /// </summary>
        Task<HealthCheckResult> RunAsync();
    }
}
