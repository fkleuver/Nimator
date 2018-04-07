using System;
using Nimator.Util;

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Default interface for all operations that need to be scheduled and/or co-ordinated in some fashion.
    /// </summary>
    public interface ITrackableOperation : IEquatable<ITrackableOperation>
    {
        /// <summary>
        /// The unique identifier for this operation.
        /// </summary>
        [NotNull]
        Identity Id { get; }

        /// <summary>
        /// Whether this operation is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Whether this operation needs to run. Should at least be the inverse of <see cref="IsRunning"/>
        /// and may include additional checks such as a minimum interval or cache duration.
        /// </summary>
        bool NeedsToRun { get; }
    }
}
