using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase.Rules
{
    public sealed class MaxTotalDocumentsInBucket : BucketsRule
    {
        public MaxTotalDocumentsInBucket(long maxDocCount) : this(new Identity(nameof(MaxTotalDocumentsInBucket)), maxDocCount) { }
        public MaxTotalDocumentsInBucket([NotNull]Identity id, long maxDocCount) : base(id)
        {
            WhenBucket(
                predicate: bucket => bucket.BasicStats.ItemCount > maxDocCount,
                actionIfTrue: (health, bucket) =>
                {
                    health
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason($"Bucket {bucket.Name} contains {bucket.BasicStats.ItemCount} items (threshold: {maxDocCount}).");
                },
                actionIfFalse: ApplyStandardOkayOperationalPolicy);
        }
    }
}
