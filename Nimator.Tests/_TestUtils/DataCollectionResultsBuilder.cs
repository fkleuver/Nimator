using System;
using AutoFixture.Kernel;

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
                return data;
            }

            return new NoSpecimen();
        }
    }
}
