using System.Threading.Tasks;

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an arbitrary health check operation that runs asynchronously.
    /// </summary>
    public interface IHealthCheck : ITrackableOperation
    {
        /// <summary>
        /// Runs this task asynchronously.
        /// </summary>
        Task RunAsync();
    }
}
