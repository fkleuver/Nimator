using System;
using System.Threading;
using System.Threading.Tasks;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Standard implementation of <see cref="T:Nimator.IDataCollector" /> that runs a data retrieval operation with an 
    /// optional timeout duration, throttled by an optional caching duration.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class DataCollector<TData> : IDataCollector where TData : class
    {
        private Exception _error;
        private readonly Func<Task<TData>> _taskFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private Task<IDataCollectionResult> _getDataAsync;

        protected readonly ILog _logger;
        // ReSharper disable once InconsistentNaming
        protected volatile bool _isRunning;

        /// <inheritdoc />
        public Identity Id { get; }

        /// <inheritdoc />
        public bool IsRunning => _isRunning;

        /// <inheritdoc />
        public bool NeedsToRun => IsStale && !_isRunning;

        public TimeSpan CacheDuration { get; }
        public bool IsStale => (NextRun ?? 0) < DateTimeProvider.GetSystemTimePrecise().Ticks;
        public long? NextRun { get; protected set; }
        public long? LastRun { get; protected set; }
        public IDataCollectionResult Result { get; private set; }

        public DataCollector(
            [NotNull]Func<Task<TData>> taskFactory,
            [CanBeNull]TimeSpan? cacheDuration = null,
            [CanBeNull]TimeSpan? timeout = null)
            : this(taskFactory, null, null, cacheDuration, timeout) { }

        public DataCollector(
            [NotNull]Func<Task<TData>> taskFactory,
            [CanBeNull]ILog logger,
            [CanBeNull]TimeSpan? cacheDuration = null,
            [CanBeNull]TimeSpan? timeout = null)
            : this(taskFactory, logger, null, cacheDuration, timeout) { }

        public DataCollector(
            [NotNull]Func<Task<TData>> taskFactory,
            [CanBeNull]Identity id,
            [CanBeNull]TimeSpan? cacheDuration = null,
            [CanBeNull]TimeSpan? timeout = null)
            : this(taskFactory, null, id, cacheDuration, timeout) { }

        public DataCollector(
            [NotNull]Func<Task<TData>> taskFactory,
            [CanBeNull]ILog logger,
            [CanBeNull]Identity id,
            [CanBeNull]TimeSpan? cacheDuration = null,
            [CanBeNull]TimeSpan? timeout = null)
        {
            Guard.AgainstNull(nameof(taskFactory), taskFactory);

            _logger = logger ?? LogProvider.GetCurrentClassLogger();
            Id = id ?? new Identity(GetType());
            CacheDuration = cacheDuration ?? TimeSpan.Zero;

            _taskFactory = async () =>
            {
                TData data = null;
                _error = null;
                try
                {
                    var task = taskFactory();
                    if (timeout.HasValue)
                    {
                        if (await Task.WhenAny(task, Task.Delay(timeout.Value)).ConfigureAwait(false) == task)
                        {
                            data = await task;
                        }
                        else
                        {
                            throw new TimeoutException($"GetAsync timed out after {timeout.Value.Milliseconds}ms.");
                        }
                    }
                    else
                    {
                        data = await task;
                    }
                }
                catch (Exception e)
                {
                    _error = e;
                    _logger.ErrorException($"[{nameof(DataCollector<TData>)}] Error on GetAsyncFunc", e);
                }

                return data;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// Runs the data retrieval task if the cache has expired, otherwise returns the existing task.
        /// This method is idempotent and can safely be called from multiple threads.
        /// </summary>
        public Task<IDataCollectionResult> GetAsync()
        {
            if (IsStale)
            {
                // We're storing the continuation of GetDataAsync in _getDataAsync so we can just
                // return the continuation to concurrent callers.
                // They will then get the same result from the same call.
                return GetDataAsync().ContinueWith(getDataAsync =>
                    {
                        _getDataAsync = getDataAsync;
                        return getDataAsync.GetAwaiter().GetResult();
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return _getDataAsync;
        }

        private async Task<IDataCollectionResult> GetDataAsync()
        {
            if (!IsStale)
            {
                _logger.Debug($"[{Id.Name}] Returning cached data");
                return Result;
            }

            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            var start = DateTimeProvider.GetSystemTimePrecise().Ticks;
            try
            {
                if (!IsStale)
                {
                    _logger.Debug($"[{Id.Name}] Returning cached data");
                    return Result;
                }
                if (_isRunning)
                {
                    _logger.Debug($"[{Id.Name}] Already running");
                    return Result;
                }

                _isRunning = true;

                _logger.Debug($"[{Id.Name}] Getting fresh data");
                var data = await _taskFactory().ConfigureAwait(false);

                LastRun = DateTimeProvider.GetSystemTimePrecise().Ticks;
                if (data != null)
                {
                    Result = new DataCollectionResult(this, start, LastRun.Value, data);
                }
                else
                {
                    Result = new DataCollectionResult(this, start, LastRun.Value, _error);
                }

            }
            catch (Exception e)
            {
                _logger.ErrorException($"[{nameof(DataCollector<TData>)}] Error on GetAsync", e);
                LastRun = DateTimeProvider.GetSystemTimePrecise().Ticks;
                Result = new DataCollectionResult(this, start, LastRun.Value, e);
            }
            finally
            {
                NextRun = DateTimeProvider.GetSystemTimePrecise().Add(CacheDuration).Ticks;
                _isRunning = false;
                _semaphoreSlim.Release();
            }

            return Result;
        }

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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataCollector<TData>)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(DataCollector<TData> left, DataCollector<TData> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DataCollector<TData> left, DataCollector<TData> right)
        {
            return !Equals(left, right);
        }
        #endregion
    }

}
