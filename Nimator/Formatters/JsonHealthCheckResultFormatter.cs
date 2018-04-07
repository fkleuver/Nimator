using Nimator.Util;

namespace Nimator.Formatters
{
    public sealed class JsonHealthCheckResultFormatter : IHealthCheckResultFormatter
    {
        public string Format([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);
            return LogSerializer.Serialize(result);
        }
    }
}
