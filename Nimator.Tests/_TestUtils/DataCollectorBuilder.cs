using System;
using System.Threading.Tasks;
using AutoFixture.Kernel;

namespace Nimator.Tests
{
    public sealed class DataCollectorBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Type type))
            {
                return new NoSpecimen();
            }

            if (type == typeof(IDataCollector))
            {
                return new DataCollector<object>(() => Task.FromResult(context.Resolve(typeof(object))));
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
            if (dataType.IsGenericParameter)
            {
                return new NoSpecimen();
            }
            var collectorType = typeof(DataCollector<>).MakeGenericType(dataType);
            if (type == collectorType)
            {
                var taskType = typeof(Task<>).MakeGenericType(dataType);
                var taskFactoryType = typeof(Func<>).MakeGenericType(taskType);
                var taskFactory = context.Resolve(taskFactoryType);
                var collectorCtor = collectorType.GetConstructor(new[] {taskFactoryType, typeof(TimeSpan?), typeof(TimeSpan?)});
                return collectorCtor.Invoke(new [] {taskFactory, TimeSpan.Zero, null});
            }

            return new NoSpecimen();
        }
    }
}
