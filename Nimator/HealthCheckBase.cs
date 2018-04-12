using System;
using System.Threading;
using System.Threading.Tasks;
using Nimator.Logging;
using Nimator.Util;
// ReSharper disable MemberCanBePrivate.Global

namespace Nimator
{
    public abstract class HealthCheckBase : IHealthCheck
    {
        protected readonly ILog Logger;
        protected int RunningCounter;
        
        public Identity Id { get; }

        public bool NeedsToRun => DateTimeProvider.GetSystemTimePrecise() >= new DateTime(LastRun.GetValueOrDefault()).AddSeconds(Interval.TotalSeconds);

        protected virtual TimeSpan Interval { get; }
        public long? LastRun { get; protected set; }

        protected HealthCheckBase([CanBeNull]ILog logger, [CanBeNull]Identity id = null, TimeSpan? interval = null)
        {
            Id = id ?? new Identity(GetType());
            Logger = logger ?? LogProvider.GetCurrentClassLogger();
            if (interval.HasValue)
            {
                Interval = interval.Value;
            }
        }
        protected HealthCheckBase([CanBeNull]Identity id = null, TimeSpan? interval = null) : this(null, id, interval) { }
        protected HealthCheckBase([NotNull]string name, TimeSpan? interval = null) : this(null,  new Identity(Guard.AgainstNullAndEmpty_Return(nameof(name), name)), interval) { }


        /// <inheritdoc />
        /// <summary>
        /// Runs this <see cref="T:Nimator.HealthCheck" /> if and only if the <see cref="P:Nimator.HealthCheck.LastRun" /> is at least <see cref="P:Nimator.HealthCheck.Interval" /> ago
        /// and if it is not already running.
        /// </summary>
        public async Task<HealthCheckResult> RunAsync()
        {
            if (!NeedsToRun)
            {
                return null;
            }
            if (Interlocked.CompareExchange(ref RunningCounter, 1, 0) != 0)
            {
                return null;
            }

            HealthCheckResult health = null;

            try
            {
                Logger.Debug($"[{Id.Name}] Running HealthCheck");
                
                try
                {
                    health = await GetHealthCheckResult();
                }
                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);

                    health = HealthCheckResult.Create(Id)
                        .SetStatus(Status.Unknown)
                        .SetLevel(LogLevel.Fatal)
                        .SetReason($"HealthCheck {GetType().Name} threw an exception in GetHealthCheckResult.")
                        .SetException(e);
                }

                Logger.Debug($"[{Id.Name}] HealthCheck complete");

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

            return health;
        }

        protected abstract Task<HealthCheckResult> GetHealthCheckResult();
    }
}
