using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nimator.Logging;
using Nimator.Messaging;
using Nimator.Util;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace Nimator
{
    /// <summary>
    /// The main entry point for running <see cref="IHealthCheck"/>s.
    /// </summary>
    public static class HealthMonitor
    {
        private static ILog _logger;
        public static ILog Logger
        {
            set => _logger = value;
            private get => _logger ?? (_logger = LogProvider.GetCurrentClassLogger());
        }

        private static readonly object _checkLock = new object();
        private static readonly object _tickLock = new object();
        private static readonly object _notifierLock = new object();

        public static readonly HashSet<IHealthCheck> Checks = new HashSet<IHealthCheck>();
        public static readonly Dictionary<INotifier, Guid> Notifiers = new Dictionary<INotifier, Guid>();

        /// <summary>
        /// A singleton thread whose sole responsibility is to run the tick loop; the actual checks run during the tick are queued
        /// to a background process provided by _taskRunner
        /// </summary>
        private static Thread _globalTickThread;

        /// <summary>
        /// Volatile ensures that _globalTickThread will see the updated value when StopTicking() is called from another thread
        /// </summary>
        private static volatile bool _isStopping;
        private static long _startTime;
        private static Action<Func<Task>> _taskRunner;
        private static Action<Func<Task>> TaskRunner => _taskRunner ?? (_taskRunner = t => ThreadPool.QueueUserWorkItem(_ => t()));

        /// <summary>
        /// Set the task runner used to queue <see cref="IHealthCheck.RunAsync"/>.
        /// </summary>
        public static void SetTaskRunner([NotNull]Action<Func<Task>> taskRunner)
        {
            _taskRunner = taskRunner;
        }

        /// <summary>
        /// Add an <see cref="IHealthCheck"/> to be run on each tick. There can only be one per <see cref="IHealthCheck"/>
        /// with the same <see cref="IHealthCheck.Id"/>. This method is idempotent and thread-safe.
        /// </summary>
        /// <returns>A boolean indicating whether the <see cref="IHealthCheck"/> was successfully added.</returns>
        public static bool AddCheck([NotNull]IHealthCheck check)
        {
            lock (_checkLock)
            {
                return Checks.Add(check);
            }
        }
        
        /// <summary>
        /// Remove the provided <see cref="IHealthCheck"/> so that it will no longer be run on each tick.
        /// This method is idempotent and thread-safe.
        /// </summary>
        /// <returns>A boolean indicating whether the <see cref="IHealthCheck"/> was successfully removed.</returns>
        public static bool RemoveCheck([NotNull]IHealthCheck check)
        {
            lock (_checkLock)
            {
                return Checks.Remove(check);
            }
        }

        /// <summary>
        /// Add an <see cref="INotifier"/> to be invoked with all created <see cref="HealthCheckResult"/>s on each tick.
        /// This method is idempotent and thread-safe.
        /// </summary>
        /// <returns>A boolean indicating whether the <see cref="IHealthCheck"/> was successfully added.</returns>
        public static bool AddNotifier([NotNull]INotifier notifier)
        {
            lock (_notifierLock)
            {
                if (Notifiers.ContainsKey(notifier))
                {
                    return false;
                }

                var subscriptionToken = EventAggregator.Instance.Subscribe<HealthCheckResult>(notifier.Send);
                Notifiers.Add(notifier, subscriptionToken);
                return true;
            }
        }
        
        /// <summary>
        /// Remove the provided <see cref="INotifier"/> so that it will no longer be invoked on each tick.
        /// This method is idempotent and thread-safe.
        /// </summary>
        /// <returns>A boolean indicating whether the <see cref="IHealthCheck"/> was successfully removed.</returns>
        public static bool RemoveNotifier([NotNull]INotifier notifier)
        {
            lock (_notifierLock)
            {
                if (!Notifiers.ContainsKey(notifier))
                {
                    return false;
                }

                var subscriptionToken = Notifiers[notifier];
                EventAggregator.Instance.Unsubscribe(subscriptionToken);
                Notifiers.Remove(notifier);
                return true;
            }
        }

        /// <summary>
        /// Starts the global ticking loop on a background thread with low priority.
        /// </summary>
        public static void StartTicking()
        {
            _startTime = DateTimeProvider.GetSystemTimePrecise().Ticks;
            Logger.Debug($"[{nameof(HealthMonitor)}] Start ticking at {_startTime}");
            Logger.Debug($"[{nameof(HealthMonitor)}] {Checks.Count} HealthChecks present");
            _globalTickThread = _globalTickThread ?? new Thread(GlobalTickLoop)
            {
                Name = "GlobalTicker",
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            if (!_globalTickThread.IsAlive)
            {
                Logger.Debug($"[{nameof(HealthMonitor)}] Starting GlobalTickThread");
                _globalTickThread.Start();
            }
            else
            {
                Logger.Debug($"[{nameof(HealthMonitor)}] GlobalTickThread already running");
            }
        }
        
        /// <summary>
        /// Stops the global ticking loop.
        /// </summary>
        public static void StopTicking()
        {
            Logger.Debug($"[{nameof(HealthMonitor)}] Stopping..");
            _isStopping = true;
        }

        private static void GlobalTickLoop()
        {
            while (!_isStopping)
            {
                try
                {
                    StartTickLoop();
                }
                catch (ThreadAbortException e)
                {
                    Logger.ErrorException($"[{nameof(HealthMonitor)}] Global ticking loop is stopping", e);
                }
                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }

                try
                {
                    Thread.Sleep(2000);
                }
                catch (ThreadAbortException)
                {
                    // ignore errors caused by app pool recycles
                }
            }
            Logger.Debug($"[{nameof(HealthMonitor)}] Exited global tick loop");
        }

        private static void StartTickLoop()
        {
            while (!_isStopping)
            {
                Tick();
                Thread.Sleep(1000);
            }
            Logger.Debug($"[{nameof(HealthMonitor)}] Exited inner tick loop");
        }

        /// <summary>
        /// Runs all checks in a "fire-and-forget" fashion, verifying with each individual <see cref="IHealthCheck"/> that it
        /// can and needs to run. This method can safely be called from any thread at any interval.
        /// </summary>
        public static void Tick()
        {
            if (!Monitor.TryEnter(_tickLock, 500))
            {
                Logger.Debug($"[{nameof(HealthMonitor)}] Unable to obtain tickLock, skipping tick");
                return;
            }

            try
            {
                if (Checks.Count == 0)
                {
                    Logger.Debug($"[{nameof(HealthMonitor)}] No HealthChecks configured");
                }
                foreach (var check in Checks)
                {
                    if (check.IsRunning)
                    {
                        Logger.Debug($"[{nameof(HealthMonitor)}] Check {check.Id.Name} is already running");
                        continue;
                    }
                    if (!check.NeedsToRun)
                    {
                        Logger.Debug($"[{nameof(HealthMonitor)}] Check {check.Id.Name} does not need to run");
                        continue;
                    }

                    Logger.Debug($"[{nameof(HealthMonitor)}] Running check {check.Id.Name}");

                    TaskRunner.Invoke(() => check.RunAsync());
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
            }
            finally
            {
                Monitor.Exit(_tickLock);
            }
        }
    }
}
