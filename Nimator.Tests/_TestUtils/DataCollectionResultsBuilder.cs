using System;
using AutoFixture.Kernel;
using Nimator.Util;
using NSubstitute;

namespace Nimator.Tests
{
    public sealed class DataCollectionResultsBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }

            if (!typeof(IDataCollectionResult).IsAssignableFrom(type))
            {
                return new NoSpecimen();
            }

            if (type == typeof(IDataCollectionResult) || type == typeof(MockDataCollectionResult))
            {
                var mock = Substitute.For<IDataCollectionResult>();
                mock.Origin.Id.Returns(new Identity(nameof(IDataCollector)));
                return new MockDataCollectionResult(mock);
            }

            if (type == typeof(DataCollectionResult))
            {
                dynamic origin = context.Resolve(typeof(IDataCollector));
                var data = origin.GetAsync().Result;
                return data;
            }

            if (!type.IsGenericType)
            {
                return new NoSpecimen();
            }

            var dataType = type.GetGenericArguments()[0];
            if (dataType.IsValueType)
            {
                return new NoSpecimen();
            }
            var resultType = typeof(DataCollectionResult<>).MakeGenericType(dataType);
            if (type == resultType)
            {
                var originType = typeof(DataCollector<>).MakeGenericType(dataType);
                dynamic origin = context.Resolve(originType);
                var data = origin.GetAsync().Result;
                var result = resultType.GetConstructor(new[] { typeof(IDataCollectionResult) })?.Invoke(new[] { data });
                return result;
            }

            return new NoSpecimen();
        }
    }
}
