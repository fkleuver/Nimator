using Nimator.Util;

namespace Nimator.Formatters
{
    public sealed class JsonHealthCheckResultFormatter : IHealthCheckResultFormatter
    {
        public string Format(HealthCheckResult result)
        {
            return LogSerializer.Serialize(result);
        }
    }
}
