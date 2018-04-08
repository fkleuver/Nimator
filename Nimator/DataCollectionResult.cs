using System;
using Nimator.Util;

namespace Nimator
{
    /// <inheritdoc cref="IDataCollectionResult" />
    public class DataCollectionResult : IDataCollectionResult
    {
        /// <inheritdoc />
        public IDataCollector Origin { get; }

        /// <inheritdoc />
        public long Start { get; }

        /// <inheritdoc />
        public long End { get; }

        /// <inheritdoc />
        public Exception Error { get; }

        /// <inheritdoc />
        public bool NeedsProcessing { get; private set; }

        /// <inheritdoc />
        public object Data { get; }

        /// <summary>
        /// Whether the operation succeeded or not.
        /// </summary>
        public bool Success => Error == null;

        public DataCollectionResult([NotNull]IDataCollectionResult clone)
        {
            Guard.AgainstNull(nameof(clone), clone);

            Origin = clone.Origin;
            Start = clone.Start;
            End = clone.End;
            Error = clone.Error;
            NeedsProcessing = clone.NeedsProcessing;
            Data = clone.Data;
        }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]Exception data) : this(origin, start, end, (object)data) { }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]object data)
        {
            Guard.AgainstNull(nameof(origin), origin);
            Guard.AgainstNull(nameof(data), data);

            Origin = origin;
            Start = start;
            End = end;
            NeedsProcessing = true;
            if (data is Exception ex)
            {
                Error = ex;
            }
            else
            {
                Data = data;
            }
        }

        public void StopProcessing()
        {
            NeedsProcessing = false;
        }
    }

    /// <inheritdoc />
    public class DataCollectionResult<TData> : DataCollectionResult where TData : class
    {
        public DataCollectionResult([NotNull]IDataCollectionResult clone) : base(clone) { }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]TData data) : base(origin, start, end, data) { }
        public DataCollectionResult([NotNull]IDataCollector origin, long start, long end, [NotNull]Exception data) : base(origin, start, end, data) { }
        public new TData Data => ((DataCollectionResult)this).Data as TData;
    }
}
