using System;

namespace Nimator
{
    /// <summary>
    /// Default interface for all <see cref="IDataCollector"/> return types.
    /// </summary>
    public interface IDataCollectionResult
    {
        /// <summary>
        /// The <see cref="IDataCollector"/> that created this instance.
        /// </summary>
        IDataCollector Origin { get; }

        /// <summary>
        /// The timestamp (ticks since 01-01-0001 00:00:00 UTC) when this operation started.
        /// </summary>
        long Start { get; }

        /// <summary>
        /// The timestamp (ticks since 01-01-0001 00:00:00 UTC) when this operation ended.
        /// </summary>
        long End { get; }

        /// <summary>
        /// The value returned by the operation. If an exception was thrown by the inner operation, this will be null.
        /// </summary>
        [CanBeNull]
        object Data { get; }

        /// <summary>
        /// The exception that was thrown by the inner operation. If no exception was thrown, this will be null.
        /// </summary>
        [CanBeNull]
        Exception Error { get; }
    }
}