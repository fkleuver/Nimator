using System;
using AutoFixture.Kernel;
using Couchbase;
using Nimator.Util;

namespace Nimator.Couchbase.Tests
{
    public sealed class ResultsBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IResult<>))
            {
                return new NoSpecimen();
            }

            var valueType = type.GetGenericArguments()[0];
            var value = valueType.Cast(context.Resolve(valueType));
            var resultType = typeof(DefaultResult<>).MakeGenericType(valueType);
            var resultCtor = resultType.GetConstructor(new[] {typeof(bool), typeof(string), typeof(Exception)});
            var result = resultCtor.Invoke(new object[] {true, "success", null});
            resultType.GetProperty("Value").GetSetMethod(true).Invoke(result, new[] {value});

            return result;
        }
    }
}
