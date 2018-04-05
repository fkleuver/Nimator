using System;
using Couchbase.Core;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase.Rules
{
    public sealed class MinAvailablePoolMemoryQuotaPercentage : ClusterRule
    {
        public MinAvailablePoolMemoryQuotaPercentage(int minPercentage) : this(new Identity(nameof(MinAvailablePoolMemoryQuotaPercentage)), minPercentage) { }
        public MinAvailablePoolMemoryQuotaPercentage(Identity id, int minPercentage) : base(id)
        {
            int GetAvailableRamPercent(IClusterInfo cluster)
            {
                var ram = cluster.Pools().StorageTotals.Ram;
                var used = (int) Math.Round((double) ram.QuotaUsed / ram.QuotaTotal * 100, 0);
                var remaining = 100 - used;
                return remaining;
            }

            WhenData(
                predicate: cluster => GetAvailableRamPercent(cluster) < minPercentage,
                actionIfTrue: (health, cluster) =>
                {
                    health
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason($"Available quota memory on group {cluster.Pools().Name} is {GetAvailableRamPercent(cluster)}% (threshhold: {minPercentage}%)");
                },
                actionIfFalse: ApplyStandardOkayOperationalPolicy);
        }
    }
}
