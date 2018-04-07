using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase.Rules
{
    public sealed class MaxDiskUsage : BucketsRule
    {
        public MaxDiskUsage(ulong maxUsed) : this(new Identity(nameof(MaxDiskUsage)), maxUsed) { }
        public MaxDiskUsage([NotNull]Identity id, ulong maxUsed) : base(id)
        {
            WhenBucket(
                predicate: bucket => bucket.BasicStats.DiskUsed > maxUsed,
                actionIfTrue: (health, bucket) =>
                {
                    health
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason($"Bucket {bucket.Name} has {bucket.BasicStats.DiskUsed} DiskUsed (threshold: {maxUsed}).");
                },
                actionIfFalse: ApplyStandardOkayOperationalPolicy);
        }
    }
}
