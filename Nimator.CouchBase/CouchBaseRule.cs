using System;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Management;
using Nimator.Logging;
using Nimator.Util;
using Result = Nimator.HealthCheckResult;

namespace Nimator.CouchBase
{

    /// <inheritdoc />
    /// <summary>
    /// A specialized subclass of <see cref="T:Nimator.HealthCheckRule`1" /> with helper methods specific
    /// to CouchBase,
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class CouchBaseRule<TData> : HealthCheckRule<IResult<TData>> where TData : class
    {
        /// <inheritdoc />
        public override bool IsMatch(object value)
        {
            return value is DataCollectionResult result && result.Data is IResult<TData>;
        }

        public CouchBaseRule(Identity checkId) : base(checkId)
        {
            WhenResult(DataCollectorFailed, (health, data) =>
            {
                health.SetLevel(LogLevel.Fatal);
                if (data.Error.GetType() == typeof(TimeoutException))
                {
                    health
                        .SetStatus(Status.Critical)
                        .SetReason($"The request to collect data from \"{data.Origin.Id}\" timed out.");
                }
                else
                {
                    health
                        .SetStatus(Status.Unknown)
                        .SetReason($"Nimator failed while trying to collect data from \"{data.Origin.Id}\".")
                        .SetErrorMessage(data.Error.Message)
                        .SetException(data.Error);
                }
            });
            WhenResult(ServiceUnresponsive, (health, data) =>
            {
                health
                    .SetStatus(Status.Critical)
                    .SetLevel(LogLevel.Error)
                    .SetReason($"Service did not respond to request from \"{data.Origin.Id}\".");
            });
        }

        public new static CouchBaseRule<TData> Create([NotNull]Identity checkId)
        {
            return new CouchBaseRule<TData>(checkId);
        }

        /// <summary>
        /// Returns true if the data was successfully retrieved from <see cref="IClusterManager"/> via <see cref="IDataCollector"/>.
        /// </summary>
        public bool ServiceResponsive(DataCollectionResult<IResult<TData>> d) => d.Success && d.Data.Success;

        /// <summary>
        /// Returns true if the <see cref="IDataCollector"/> threw no exception, but data retrieval was unsuccessful.
        /// </summary>
        public bool ServiceUnresponsive(DataCollectionResult<IResult<TData>> d) => d.Success && !d.Data.Success;

        
        /// <summary>
        /// Manipulates a <see cref="Result"/> associated with a <see cref="IResult{T}"/> if it satisfies a provided condition.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether the action should be executed or not.</param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<IResult<TData>> WhenData(
            Predicate<TData> predicate,
            Action<Result, TData> actionIfTrue,
            Action<Result, TData> actionIfFalse = null)
        {
            return WhenResult(
                predicate: dataResult => ServiceResponsive(dataResult) && predicate(dataResult.Data.Value),
                actionIfTrue: (health, data) => { actionIfTrue(health, data.Data.Value); },
                actionIfFalse: (health, data) => { actionIfFalse?.Invoke(health, data.Data.Value); });
        }

        /// <summary>
        /// Applies this rule to a sequence of child elements, and adds the resulting <see cref="Result"/> instances
        /// to the InnerResults property of a new parent <see cref="Result"/> which itself will be empty.
        /// </summary>
        /// <param name="query">The function to select the child items from the parent item.</param>
        /// <param name="propertyRules">A rule (or optionally, sequence of rules) to apply to each individual child item.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<IResult<TData>> ForEachDataProperty<TProperty>(
            Func<TData, IEnumerable<TProperty>> query,
            params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
        {
            return ForEachDataProperty(
                predicate: Always,
                query: query,
                parentAction: DoNothing,
                propertyRules: propertyRules);
        }

        /// <summary>
        /// Applies this rule to a sequence of child elements, and adds the resulting <see cref="Result"/> instances
        /// to the InnerResults property of a new parent <see cref="Result"/> which itself will be empty.
        /// </summary>
        /// <param name="predicate">
        /// The predicate to determine whether the action should be executed or not.
        /// The parent node is tested, and the outcome will result in either all, or none of the child items being processed.
        /// </param>
        /// <param name="query">The function to select the child items from the parent item.</param>
        /// <param name="propertyRules">A rule (or optionally, sequence of rules) to apply to each individual child item.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<IResult<TData>> ForEachDataProperty<TProperty>(
            Predicate<TData> predicate,
            Func<TData, IEnumerable<TProperty>> query,
            params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
        {
            return ForEachDataProperty(
                predicate: predicate,
                query: query,
                parentAction: DoNothing,
                propertyRules: propertyRules);
        }

        /// <summary>
        /// Applies this rule to a sequence of child elements, and adds the resulting <see cref="Result"/> instances
        /// to the InnerResults property of a new parent <see cref="Result"/> which itself will be empty.
        /// </summary>
        /// <param name="predicate">
        /// The predicate to determine whether the action should be executed or not.
        /// This predicate evaluates the parent node, and the outcome will result in either all or none of the child items being processed.
        /// </param>
        /// <param name="query">The function to select the child items from the parent item.</param>
        /// <param name="parentAction">
        /// The action to perform on the (empty) parent <see cref="Result"/> after all the inner results are added to it.
        /// </param>
        /// <param name="propertyRules">A rule (or optionally, sequence of rules) to apply to each individual child item.</param>
        /// <returns></returns>
        public HealthCheckRule<IResult<TData>> ForEachDataProperty<TProperty>(
            Predicate<TData> predicate,
            Func<TData, IEnumerable<TProperty>> query,
            Action<Result, TData> parentAction,
            params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
        {
            void Action(Result result, TData data)
            {
                foreach (var childItem in query(data))
                {
                    foreach (var rule in propertyRules)
                    {
                        var childResult = new Result(CheckId);
                        if (rule.Item1(childItem))
                        {
                            rule.Item2(childResult, childItem);
                        }
                        else
                        {
                            rule.Item3(childResult, childItem);
                        }
                        result.InnerResults.Add(childResult);
                    }
                }

                parentAction(result, data);
            }

            InnerRules.Add(
                key: result => predicate(result.Data.Value), 
                value: (health, data) => Action(health, data.Data.Value));

            return this;
        }
    }
}
