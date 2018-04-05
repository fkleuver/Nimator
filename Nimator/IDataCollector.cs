using System.Threading.Tasks;

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an arbitrary data collection operation that runs asynchronously.
    /// </summary>
    public interface IDataCollector : ITrackableOperation
    {
        /// <summary>
        /// Gets the data asynchronously.
        /// </summary>
        Task<IDataCollectionResult> GetAsync();
    }
}
