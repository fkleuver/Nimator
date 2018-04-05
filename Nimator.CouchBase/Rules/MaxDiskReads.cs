using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase.Rules
{
    public sealed class MaxDiskReads : BucketsRule
    {
        public MaxDiskReads(double maxReads) : this(new Identity(nameof(MaxDiskReads)), maxReads) { }
        public MaxDiskReads(Identity id, double maxReads) : base(id)
        {
            WhenBucket(
                predicate: bucket => bucket.BasicStats.DiskFetches > maxReads,
                actionIfTrue: (health, bucket) =>
                {
                    health
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason($"Bucket {bucket.Name} has {bucket.BasicStats.DiskFetches} DiskFetches (threshold: {maxReads}).");
                },
                actionIfFalse: ApplyStandardOkayOperationalPolicy);
        }
    }
}
