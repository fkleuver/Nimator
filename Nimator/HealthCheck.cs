using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nimator.Logging;
using Nimator.Messaging;
using Nimator.Util;
// ReSharper disable MemberCanBePrivate.Global

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Standard implementation of <see cref="T:Nimator.IHealthCheck" /> that runs a list of <see cref="T:Nimator.IHealthCheckRule" /> over the results
    /// of a list of <see cref="T:Nimator.IDataCollector" />, combining all the <see cref="T:Nimator.HealthCheckResult" />s into a single parent result
    /// and then publishing that result via the <see cref="T:Nimator.Messaging.EventAggregator" />.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IEquatable{HealthCheck}"/> and will compare as being equal to
    /// any other <see cref="HealthCheck"/> with the same <see cref="Id"/>/
    /// </remarks>
    public class HealthCheck : IHealthCheck
    {
        protected readonly ILog Logger;
        protected int RunningCounter;

        protected virtual int MinimumTickIntervalSeconds { get; set; } = 15;

        public Guid Guid { get; }

        /// <inheritdoc />
        public Identity Id { get; }

        /// <inheritdoc />
        public bool IsRunning => RunningCounter > 0;

        /// <inheritdoc />
        public bool NeedsToRun => DateTimeProvider.GetSystemTimePrecise() >= new DateTime(LastRun.GetValueOrDefault()).AddSeconds(MinimumTickIntervalSeconds);
        
        public long? LastRun { get; protected set; }
        public IList<IDataCollector> DataCollectors { get; } = new List<IDataCollector>();
        public IList<IHealthCheckRule> Rules { get; } = new List<IHealthCheckRule>();

        public HealthCheck([CanBeNull]ILog logger, [CanBeNull]Identity id = null, int? interval = null)
        {
            Id = id ?? new Identity(GetType());
            Logger = logger ?? LogProvider.GetCurrentClassLogger();
            Guid = GuidGenerator.GenerateTimeBasedGuid();
            if (interval.HasValue)
            {
                MinimumTickIntervalSeconds = interval.Value;
            }
        }
        public HealthCheck([CanBeNull]Identity id = null, int? interval = null) : this(null, id, interval) { }
        public HealthCheck([NotNull]string name, int? interval = null) : this(null,  new Identity(name), interval) { }


        /// <inheritdoc />
        /// <summary>
        /// Runs this <see cref="T:Nimator.HealthCheck" /> if and only if the <see cref="P:Nimator.HealthCheck.LastRun" /> is at least <see cref="P:Nimator.HealthCheck.MinimumTickIntervalSeconds" /> ago
        /// and if it is not already running.
        /// </summary>
        public async Task RunAsync()
        {
            if (!NeedsToRun)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref RunningCounter, 1, 0) != 0)
            {
                return;
            }

            Logger.Debug($"[{Id.Name}] Running HealthCheck {Id.Name}");
            try
            {
                Logger.Debug($"[{Id.Name}] Queueing DataCollectors");
                var tasks = new List<Task<IDataCollectionResult>>();
                foreach (var dataCollector in DataCollectors)
                {
                    if (dataCollector.NeedsToRun && !dataCollector.IsRunning)
                    {
                        tasks.Add(dataCollector.GetAsync());
                    }
                }

                if (tasks.Count == 0)
                {
                    Logger.Debug($"[{Id.Name}] DataCollectors complete (none to run)");
                    // Setting LastRun here to "reset" the loop timer, but we could also omit this to ensure
                    // the DataCollectors are called as soon as they need to be called (rather than at the next parent threshold)
                    LastRun = DateTimeProvider.GetSystemTimePrecise().Ticks;
                    return;
                }

                Logger.Debug($"[{Id.Name}] DataCollectors queued (now awaiting)");

                var dataResults = await Task.WhenAll(tasks).ConfigureAwait(false);

                Logger.Debug($"[{Id.Name}] DataCollectors complete (awaited)");

                Logger.Debug($"[{Id.Name}] Processing rules started");

                var checkResult = new HealthCheckResult(Id);
                foreach (var rule in Rules)
                {
                    var currentRuleIsMatched = false;
                    foreach (var dataResult in dataResults)
                    {
                        try
                        {
                            if (rule.IsMatch(dataResult))
                            {
                                try
                                {
                                    var innerCheckResult = rule.GetResult(dataResult);
                                    if (innerCheckResult != null)
                                    {
                                        currentRuleIsMatched = true;
                                        checkResult.AddInnerResult(innerCheckResult);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.ErrorException(e.Message, e);

                                    checkResult.AddInnerResult(inner => inner
                                        .SetStatus(Status.Unknown)
                                        .SetLevel(LogLevel.Fatal)
                                        .SetReason($"Rule {rule.GetType().Name} threw an exception in GetResult.")
                                        .SetException(e));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorException(e.Message, e);

                            checkResult.AddInnerResult(inner => inner
                                .SetStatus(Status.Unknown)
                                .SetLevel(LogLevel.Fatal)
                                .SetReason($"Rule {rule.GetType().Name} threw an exception in IsMatch.")
                                .SetException(e));
                        }
                    }
                    if (!currentRuleIsMatched)
                    {
                        var message = $"Rule {rule.GetType().Name} was not matched against any of the collected data this tick.";
                        Logger.Warn($"[{Id.Name}] {message}");

                        checkResult.AddInnerResult(inner => inner
                            .SetStatus(Status.Unknown)
                            .SetLevel(LogLevel.Warn)
                            .SetReason(message));
                    }
                }

                Logger.Debug($"[{Id.Name}] Processing rules complete");

                EventAggregator.Instance.Publish(checkResult);

                LastRun = DateTimeProvider.GetSystemTimePrecise().Ticks;
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
            }
            finally
            {
                Interlocked.Exchange(ref RunningCounter, 0);
            }
            Logger.Debug($"[{Id.Name}] HealthCheck complete");

        }

        /// <summary>
        /// Adds a <see cref="IDataCollector"/> to this check. The output of the collector is fed
        /// to all of this check's <see cref="IHealthCheckRule"/>s on each tick.
        /// </summary>
        public HealthCheck AddDataCollector([NotNull]IDataCollector dataCollector)
        {
            Guard.AgainstNull(nameof(dataCollector), dataCollector);

            DataCollectors.Add(dataCollector);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IDataCollector"/> to this check. The output of the collector is fed
        /// to all of this check's <see cref="IHealthCheckRule"/>s on each tick.
        /// </summary>
        public HealthCheck AddDataCollector<TData>(
            [NotNull]Func<Task<TData>> taskFactory,
            [CanBeNull]TimeSpan? cacheDuration = null,
            [CanBeNull]TimeSpan? timeout = null) where TData : class
        {
            Guard.AgainstNull(nameof(taskFactory), taskFactory);
            
            var collector = new DataCollector<TData>(taskFactory, cacheDuration, timeout);
            DataCollectors.Add(collector);
            return this;
        }
        
        /// <summary>
        /// Adds a <see cref="IHealthCheckRule"/> to this check. The output of all of this check's
        /// <see cref="IDataCollector"/>s will be fed to the rule on each tick.
        /// </summary>
        public HealthCheck AddRule([NotNull]IHealthCheckRule rule)
        {
            Guard.AgainstNull(nameof(rule), rule);

            Rules.Add(rule);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IHealthCheckRule"/> to this check. The output of all of this check's
        /// <see cref="IDataCollector"/>s will be fed to the rule on each tick.
        /// </summary>
        public HealthCheck AddRule<TData>([NotNull]Action<HealthCheckRule<TData>> configureRule) where TData : class
        {
            Guard.AgainstNull(nameof(configureRule), configureRule);

            var rule = new HealthCheckRule<TData>(Id);
            configureRule(rule);
            Rules.Add(rule);
            return this;
        }

        #region IDisposable implementation
        public void Dispose()
        {
            HealthMonitor.RemoveCheck(this);
        }
        #endregion

        #region IEquatable implementation
        public bool Equals(ITrackableOperation other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HealthCheck)obj);
        }

        public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;

        public static bool operator ==(HealthCheck left, HealthCheck right) => Equals(left, right);

        public static bool operator !=(HealthCheck left, HealthCheck right) => !Equals(left, right);
        #endregion
    }
}
