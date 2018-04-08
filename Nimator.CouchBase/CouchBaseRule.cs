using System;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Management;
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
        public override bool IsMatch([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            if (!(value is DataCollectionResult result))
            {
                return false;
            }

            return result.Data is IResult<TData>;
        }

        public CouchBaseRule([NotNull]Identity checkId) : base(checkId) { }

        public new static CouchBaseRule<TData> Create([NotNull]Identity checkId)
        {
            return new CouchBaseRule<TData>(checkId);
        }

        /// <summary>
        /// Returns true if the data was successfully retrieved from <see cref="IClusterManager"/> via <see cref="IDataCollector"/>.
        /// </summary>
        public bool ServiceResponsive([NotNull]DataCollectionResult<IResult<TData>> d) => Guard.AgainstNull_Return(nameof(d), d).Success && d.Data.Success;

        /// <summary>
        /// Returns true if the <see cref="IDataCollector"/> threw no exception, but data retrieval was unsuccessful.
        /// </summary>
        public bool ServiceUnresponsive([NotNull]DataCollectionResult<IResult<TData>> d) => Guard.AgainstNull_Return(nameof(d), d).Success && !d.Data.Success;


        /// <summary>
        /// Manipulates a <see cref="Result"/> associated with a <see cref="IResult{T}"/> if it satisfies a provided condition.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether the action should be executed or not.</param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<IResult<TData>> WhenData(
            [NotNull]Predicate<TData> predicate,
            [NotNull]Action<Result, TData> actionIfTrue,
            [CanBeNull]Action<Result, TData> actionIfFalse = null)
        {
            Guard.AgainstNull(nameof(predicate), predicate);
            Guard.AgainstNull(nameof(actionIfTrue), actionIfTrue);

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
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            [NotNull, NotEmpty, ItemNotNull]params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules) where TProperty : class
        {
            Guard.AgainstNull(nameof(query), query);
            Guard.AgainstNullAndEmpty(nameof(propertyRules), propertyRules);
            foreach (var propertyRule in propertyRules)
            {
                Guard.AgainstNull(nameof(propertyRule), propertyRule);
            }

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
            [NotNull]Predicate<TData> predicate,
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules) where TProperty : class
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
            [NotNull]Predicate<TData> predicate,
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            [NotNull]Action<Result, TData> parentAction,
            [NotNull, NotEmpty, ItemNotNull]params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules) where TProperty : class
        {
            Guard.AgainstNull(nameof(predicate), predicate);
            Guard.AgainstNull(nameof(query), query);
            Guard.AgainstNull(nameof(parentAction), parentAction);
            Guard.AgainstNullAndEmpty(nameof(propertyRules), propertyRules);
            foreach (var propertyRule in propertyRules)
            {
                Guard.AgainstNull(nameof(propertyRule), propertyRule);
            }

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
