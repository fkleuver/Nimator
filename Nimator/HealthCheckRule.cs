using System;
using System.Collections.Generic;
using Nimator.Logging;
using Nimator.Util;
using Result = Nimator.HealthCheckResult;

namespace Nimator
{
    /// <inheritdoc />
    public class HealthCheckRule<TData> : IHealthCheckRule where TData : class
    {
        protected readonly Identity CheckId;
        
        protected readonly Dictionary<
            Predicate<DataCollectionResult<TData>>,
            Action<Result, DataCollectionResult<TData>>
        > InnerRules = new Dictionary<
            Predicate<DataCollectionResult<TData>>,
            Action<Result, DataCollectionResult<TData>>
        >();
        
        public HealthCheckRule([NotNull]string name) : this(new Identity(name)) { }
        /// <inheritdoc />
        public HealthCheckRule([NotNull]Identity checkId)
        {
            Guard.AgainstNull(nameof(checkId), checkId);

            CheckId = checkId;
        }

        public static HealthCheckRule<TData> Create([NotNull]Identity checkId)
        {
            return new HealthCheckRule<TData>(checkId);
        }
        
        /// <inheritdoc />
        public virtual bool IsMatch([NotNull]object value)
        {
            Guard.AgainstNull(nameof(value), value);

            return value is DataCollectionResult dcr1 && (dcr1.Data == null || dcr1.Data is TData);
        }

        /// <inheritdoc />
        public virtual Result GetResult([NotNull]object dataResult)
        {
            Guard.AgainstNull(nameof(dataResult), dataResult);

            if (!IsMatch(dataResult))
            {
                throw new ArgumentException(
                    $"{nameof(HealthCheckResult)} received a data object of the wrong type: {dataResult.GetType().Name}");
            }

            // This is very ugly; need to make it better
            var typedDataResult = Convert.ChangeType(dataResult, typeof(DataCollectionResult<TData>)) as DataCollectionResult<TData>;
            var hasMatch = false;
            var result = new Result(CheckId);
            foreach (var predicate in InnerRules.Keys)
            {
                if (predicate(typedDataResult))
                {
                    hasMatch = true;
                    var applyInnerRule = InnerRules[predicate];
                    applyInnerRule(result, typedDataResult);
                }
            }

            return hasMatch ? result : null;
        }

        /// <summary>
        /// Manipulates a <see cref="Result"/> associated with a <see cref="DataCollectionResult{TData}"/> if it satisfies a provided condition.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether the action should be executed or not.</param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<TData> WhenResult(
            [NotNull]Predicate<DataCollectionResult<TData>> predicate,
            [NotNull]Action<Result, DataCollectionResult<TData>> actionIfTrue,
            [CanBeNull]Action<Result, DataCollectionResult<TData>> actionIfFalse = null)
        {
            Guard.AgainstNull(nameof(predicate), predicate);
            Guard.AgainstNull(nameof(actionIfTrue), actionIfTrue);

            InnerRules.Add(predicate, actionIfTrue);
            if (actionIfFalse != null)
            {
                InnerRules.Add(data => !predicate(data), actionIfFalse);
            }

            return this;
        }


        /// <summary>
        /// Manipulates a <see cref="Result"/> associated with a <see cref="TData"/> if the <see cref="DataCollector{TData}"/>
        /// succeeded in retrieving the data and it satisfies a provided condition.
        /// It is therefore (typically) safe to assume that there is data.
        /// </summary>
        /// <param name="predicate">
        /// The predicate to determine whether the action should be executed or not.
        /// This predicate is appended to a check whether the <see cref="DataCollector{TData}"/> succeeded,
        /// and will always return false if it didn't.
        /// </param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<TData> WhenResultSucceededAndWhenData(
            [NotNull]Predicate<TData> predicate,
            [NotNull]Action<Result, TData> actionIfTrue,
            [CanBeNull]Action<Result, TData> actionIfFalse = null)
        {
            Guard.AgainstNull(nameof(predicate), predicate);
            Guard.AgainstNull(nameof(actionIfTrue), actionIfTrue);

            return WhenResult(
                predicate: dataResult => DataCollectorSuccess(dataResult) && predicate(dataResult.Data),
                actionIfTrue: (health, data) => { actionIfTrue(health, data.Data); },
                actionIfFalse: (health, data) => { actionIfFalse?.Invoke(health, data.Data); });
        }


        /// <summary>
        /// Applies this rule to a sequence of child elements, and adds the resulting <see cref="Result"/> instances
        /// to the InnerResults property of a new parent <see cref="Result"/> which itself will be empty.
        /// </summary>
        /// <param name="query">The function to select the child items from the parent item.</param>
        /// <param name="propertyRules">A rule (or optionally, sequence of rules) to apply to each individual child item.</param>
        /// <returns>This <see cref="HealthCheckRule{TData}"/> instance.</returns>
        public HealthCheckRule<TData> ForEachProperty<TProperty>(
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            [NotNull]params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
        {
            return ForEachProperty(
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
        public HealthCheckRule<TData> ForEachProperty<TProperty>(
            [NotNull]Predicate<TData> predicate,
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            [NotNull]params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
        {
            return ForEachProperty(
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
        public HealthCheckRule<TData> ForEachProperty<TProperty>(
            [NotNull]Predicate<TData> predicate,
            [NotNull]Func<TData, IEnumerable<TProperty>> query,
            [NotNull]Action<Result, TData> parentAction,
            [NotNull, NotEmpty, ItemNotNull]params Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >[] propertyRules)
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
                key: result => predicate(result.Data),
                value: (health, data) => Action(health, data.Data));

            return this;
        }

        /// <summary>
        /// Applies this rule to an individual child element, when within a <see cref="ForEachProperty"/> function.
        /// Essentially just a wrapper function that returns a tuple back to that function.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether the action should be executed or not.</param>
        /// <param name="actionIfTrue">The action to execute if the predicate evaluates to true.</param>
        /// <param name="actionIfFalse">Optional. The action to execute if the predicate evaluates to false.</param>
        /// <returns>The <see cref="Tuple"/> needed by the outer <see cref="ForEachProperty"/> function</returns>
        public Tuple<
            Predicate<TProperty>,
            Action<Result, TProperty>,
            Action<Result, TProperty>
            >
            WhenProperty<TProperty>(
            [NotNull]Predicate<TProperty> predicate,
            [NotNull]Action<Result, TProperty> actionIfTrue,
            [CanBeNull]Action<Result, TProperty> actionIfFalse = null)
        {
            Guard.AgainstNull(nameof(predicate), predicate);
            Guard.AgainstNull(nameof(actionIfTrue), actionIfTrue);

            return new Tuple<
                Predicate<TProperty>,
                Action<Result, TProperty>,
                Action<Result, TProperty>
            >(
                predicate,
                actionIfTrue,
                actionIfFalse ?? DoNothing);
        }

        /// <summary>
        /// A predicate that always returns true, regardless of the value of <see cref="obj"/>.
        /// </summary>
        public bool Always<T>(T obj) => true;

        /// <summary>
        /// A predicate that always returns false, regardless of the value of <see cref="obj"/>.
        /// </summary>
        public bool Never<T>(T obj) => false;

        /// <summary>
        /// A query that literally returns the value that is passed in.
        /// </summary>
        public T PassThrough<T>(T obj) => obj;

        /// <summary>
        /// An action that does nothing.
        /// </summary>
        public void DoNothing<T>(T obj) { }

        /// <summary>
        /// An action that does nothing.
        /// </summary>
        public void DoNothing<T1, T2>(T1 arg1, T2 arg2) { }

        /// <summary>
        /// An action that does nothing.
        /// </summary>
        public void DoNothing<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) { }

        /// <summary>
        /// A predicate that returns the value of <see cref="DataCollectionResult.Success"/>.
        /// </summary>
        public bool DataCollectorSuccess([NotNull]DataCollectionResult<TData> dataCollectionResult)
        {
            Guard.AgainstNull(nameof(dataCollectionResult), dataCollectionResult);

            return dataCollectionResult.Success;
        }

        /// <summary>
        /// A predicate that returns the inverse of <see cref="DataCollectionResult.Success"/>.
        /// </summary>
        public bool DataCollectorFailed([NotNull]DataCollectionResult<TData> dataCollectionResult)
        {
            Guard.AgainstNull(nameof(dataCollectionResult), dataCollectionResult);

            return !dataCollectionResult.Success;
        }
        
        /// <summary>
        /// An action that sets <see cref="Status.Okay"/> and <see cref="LogLevel.Info"/>
        /// on the result.
        /// </summary>
        public void ApplyStandardOkayOperationalPolicy([NotNull]Result healthCheckResult, [CanBeNull]TData data)
        {
            Guard.AgainstNull(nameof(healthCheckResult), healthCheckResult);

            healthCheckResult.SetStatus(Status.Okay).SetLevel(LogLevel.Info);
        }
        
        /// <summary>
        /// An action that sets <see cref="Status.Okay"/> and <see cref="LogLevel.Info"/>
        /// on the result.
        /// </summary>
        public void ApplyStandardOkayOperationalPolicy<TProperty>([NotNull]Result healthCheckResult, [CanBeNull]TProperty property)
        {
            Guard.AgainstNull(nameof(healthCheckResult), healthCheckResult);

            healthCheckResult.SetStatus(Status.Okay).SetLevel(LogLevel.Info);
        }
    }
}
