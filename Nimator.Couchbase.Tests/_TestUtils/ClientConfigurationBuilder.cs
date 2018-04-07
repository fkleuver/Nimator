using System;
using AutoFixture.Kernel;
using Couchbase.Configuration.Client;

namespace Nimator.Couchbase.Tests
{
    public sealed class ClientConfigurationBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }
            if (type != typeof(ClientConfiguration))
            {
                return new NoSpecimen();
            }

            return new ClientConfiguration();
        }
    }
}
