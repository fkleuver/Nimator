using System;
using AutoFixture.Kernel;
using Couchbase;
using NSubstitute;

namespace Nimator.Couchbase.Tests
{
    public sealed class MockResultsBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(MockResult<>))
            {
                return new NoSpecimen();
            }

            var mock = Substitute.For<IResult<object>>();
            return new MockResult<object>(mock);
        }
    }
}
