using System;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using Nimator.Util;

namespace Nimator.CouchBase
{
    /// <summary>
    /// A marker subclass of <see cref="CouchBaseRule"/> specifically for processing <see cref="BucketConfig"/> results.
    /// </summary>
    public class BucketsRule : CouchBaseRule<IList<BucketConfig>>
    {
        protected BucketsRule(Identity checkId) : base(checkId)
        {
        }

        public new static BucketsRule Create([NotNull]Identity checkId)
        {
            return new BucketsRule(checkId);
        }

        public override bool IsMatch(object value)
        {
            if (base.IsMatch(value))
            {
                return true;
            }
            if (!(value is DataCollectionResult dcr1))
            {
                return false;
            }

            if (dcr1.Data is IResult<IList<BucketConfig>>)
            {
                return true;
            }

            if (dcr1.Data == null)
            {
                return false;
            }

            var dataType = dcr1.Data.GetType();
            if (!dataType.IsGenericType)
            {
                return false;
            }

            var genericDataType = dataType.GetGenericTypeDefinition();
            if (genericDataType.Name != "IResult`1")
            {
                var dataTypeInterface = genericDataType.GetInterface("IResult`1");
                if (dataTypeInterface == null)
                {
                    return false;
                }
            }

            var valueType = dataType.GetGenericArguments()[0];
            if (!valueType.IsGenericType)
            {
                return false;
            }

            var genericValueType = valueType.GetGenericTypeDefinition();
            if (genericValueType.Name != "IList`1")
            {
                var valueTypeInterface = genericValueType.GetInterface("IList`1");
                if (valueTypeInterface == null)
                {
                    return false;
                }
            }

            var itemType = valueType.GetGenericArguments()[0];
            return itemType == typeof(BucketConfig);
        }
        
        /// <summary>
        /// Manipulates a <see cref="HealthCheckResult"/> associated with a <see cref="IResult{T}"/> if it satisfies a provided condition.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether the action should be executed or not.</param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<IResult<IList<BucketConfig>>> WhenBucket(
            Predicate<BucketConfig> predicate,
            Action<HealthCheckResult, BucketConfig> actionIfTrue,
            Action<HealthCheckResult, BucketConfig> actionIfFalse = null)
        {
            return ForEachDataProperty(
                predicate: Always,
                query: PassThrough,
                parentAction: DoNothing,
                propertyRules: WhenProperty(
                    predicate: predicate,
                    actionIfTrue: actionIfTrue,
                    actionIfFalse: actionIfFalse));
        }
    }
}