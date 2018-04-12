# Quick start

Run the project `Nimator.ConsoleHost`. 

If you're debugging, VS may break on an exception (e.g. failed login). Just press continue if this happens; the engine will handle it.

To make the CouchBase Health check work, add a valid CouchBase Username and Password to App.config.

# Introduction

Much like the original Nimator, this library revolves around defining `HealthChecks` to be run on certain intervals.

This is a simplified version of v0.1 with most of the auxiliary classes and functions omitted.

## Defining a CouchBase HealthCheck:

Two helper base classes are available for checking clusters and buckets.

They will handle application-, configuration- and network-level issues so that the check can focus on the variable stuff.

```csharp
public class BucketsHealthCheck : BucketsHealthCheckBase
{
    // override the default demo interval of 10 seconds
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(30);

    // get the credentials from app.config by default
    public BucketsHealthCheck() : base(ClusterManagerFactory.FromAppSettings(AppSettings.FromConfigurationManager())) { }

    // per interval, this method will be called for each bucket in the cluster and the bucket results are
    // concatenated to a single parent result
    protected override Task<HealthCheckResult> GetHealthCheckResult(IBucketConfig bucket)
    {
        var health = HealthCheckResult.Create(bucket.Name);
        if (bucket.BasicStats.ItemCount > 100000)
        {
            health.SetStatus(Status.Warning).SetLevel(LogLevel.Warn).SetReason($"Bucket {bucket.Name} has more than 100000 documents.");
        }
        else
        {
            health.SetStatus(Status.Okay);
        }

        return Task.FromResult(health);
    }
}
```

## Defining a custom HealthCheck:

The `HealthCheckBase` class works similar to the CouchBase base classes and provides most of the same
common functionality.

For checks that require a different kind of errorhandling or timing the `IHealthCheck` interface can be implemented like so:

```csharp
public class CustomHealthCheck : IHealthCheck
{
    // the Id is included in logging output and used to determine uniqueness
    public Identity Id { get; } = new Identity("Custom");

    // HealthMonitor will check this every second, and only call this check when this property returns true
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
```

## Running the health monitor

This example code comes directly from `Nimator.ConsoleHost\Program.cs`

```csharp
// initialize LibLog with the Serilog implementation from app.config settings
Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithThreadId().CreateLogger();

// add some health checks
HealthMonitor.AddCheck(new BucketsHealthCheck());
HealthMonitor.AddCheck(new ClusterHealthCheck());
HealthMonitor.AddCheck(new PingHealthCheck());

// add one or more notifiers
HealthMonitor.AddNotifier(LibLogNotifierSettings.Create().ToNotifier());

// start running on a background thread
HealthMonitor.StartTicking();

```
