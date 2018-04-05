using System;
using System.Collections.Generic;
using AutoFixture.Kernel;
using Couchbase.Configuration.Server.Serialization;

namespace Nimator.Couchbase.Tests
{
    public sealed class BucketsBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }
            if (type != typeof(IList<BucketConfig>))
            {
                return new NoSpecimen();
            }

            var buckets = new List<BucketConfig>() as IList<BucketConfig>;
            for (var i = 0; i < 3; i++)
            {
                buckets.Add(context.Resolve(typeof(BucketConfig)) as BucketConfig);
            }

            return buckets;
        }
    }
}
