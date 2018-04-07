using Nimator.Util;

namespace Nimator.Formatters
{
    public sealed class PlainTextFormatter : IHealthCheckResultFormatter
    {
        public string Format([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);
            return $"{result.Level} in {result.CheckId.Name}: {result.Reason}";
        }
    }
}
