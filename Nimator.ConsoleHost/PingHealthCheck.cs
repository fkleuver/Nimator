using System;
using System.Threading.Tasks;
using Nimator.Util;

namespace Nimator.ConsoleHost
{
    public sealed class PingHealthCheck : IHealthCheck
    {
        public Identity Id { get; }
        public bool NeedsToRun => DateTime.UtcNow > _lastRun.AddSeconds(3);
        private DateTime _lastRun = DateTime.MinValue;

        public PingHealthCheck()
        {
            Id = new Identity(GetType());
        }

        public Task<HealthCheckResult> RunAsync()
        {
            _lastRun = DateTime.UtcNow;
            return Task.FromResult(HealthCheckResult.Create(Id).SetStatus(Status.Okay).SetReason("Pong"));
        }
    }
}
