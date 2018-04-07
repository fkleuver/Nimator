using Couchbase.Core;
using Nimator.Util;

namespace Nimator.CouchBase
{
    /// <summary>
    /// A marker subclass of <see cref="CouchBaseRule"/> specifically for processing <see cref="IClusterInfo"/> results.
    /// </summary>
    public class ClusterRule : CouchBaseRule<IClusterInfo>
    {
        public ClusterRule([NotNull]Identity checkId) : base(checkId)
        {
        }

        public new static ClusterRule Create([NotNull]Identity checkId)
        {
            return new ClusterRule(checkId);
        }
    }
}