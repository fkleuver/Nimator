using System;
using System.Collections.Generic;
using AutoFixture.Kernel;

namespace Nimator.Tests
{
    public sealed class GuardClauseFailingDataBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is Tuple<Type, Attribute> tuple))
            {
                return new NoSpecimen();
            }

            var paramType = tuple.Item1;
            var attributeType = tuple.Item2.GetType();

            if (attributeType == typeof(NotNullAttribute) || attributeType == typeof(CanBeNullAttribute))
            {
                return null;
            }

            if (attributeType == typeof(ItemNotNullAttribute) || attributeType == typeof(ItemCanBeNullAttribute) || attributeType == typeof(CanBeEmptyAttribute) || attributeType == typeof(NotEmptyAttribute))
            {
                Type itemType;
                if (paramType.IsArray)
                {
                    itemType = paramType.HasElementType ? paramType.GetElementType() : typeof(object);
                }
                else
                {
                    itemType = paramType.IsGenericType ? paramType.GetGenericArguments()[0] : typeof(object);
                }

                // ReSharper disable once PossibleNullReferenceException
                dynamic list = typeof(List<>).MakeGenericType(itemType).GetConstructor(new Type[0]).Invoke(new object[0]);
                if (attributeType == typeof(ItemNotNullAttribute) || attributeType == typeof(ItemCanBeNullAttribute))
                {
                    list.Add(null);
                }
                return paramType.IsArray ? list.ToArray() : list;
            }

            return new NoSpecimen();
        }
    }
}
