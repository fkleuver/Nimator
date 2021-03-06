﻿using System.Collections.Generic;
using AutoFixture.Kernel;
using Nimator.Tests;

namespace Nimator.Couchbase.Tests
{
    public class CouchBaseFixture : DefaultFixture
    {
        protected override IEnumerable<ISpecimenBuilder> GetBuilders()
        {
            foreach (var builder in base.GetBuilders())
            {
                yield return builder;
            }
            yield return new ClientConfigurationBuilder();
        }

        public new static ISpecimenContext CreateContext()
        {
            return new SpecimenContext(new CouchBaseFixture());
        }
    }
}
