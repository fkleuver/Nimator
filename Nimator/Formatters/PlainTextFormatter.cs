namespace Nimator.Formatters
{
    public sealed class PlainTextFormatter : IHealthCheckResultFormatter
    {
        public string Format(HealthCheckResult result)
        {
            return $"{result.Level} in {result.CheckId.Name}: {result.Reason}";
        }
    }
}
