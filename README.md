# Quick start

Run the project `Nimator.ConsoleHost`. 

If you're debugging, VS may break on an exception (e.g. failed login). Just press continue if this happens; the engine will handle it.

To make the CouchBase Health check work, add a valid CouchBase Username and Password to App.config.

# Introduction

Much like the original Nimator, this library revolves around defining `HealthChecks` to be run on certain intervals. The concept of HealthChecks is expanded upon via a Fluent API that allows you to compose HealthChecks out of smaller pieces intended to make client code more expressive and concise, as well as reducing boilerplate code.

New concepts are `DataCollectors` which fetch data from various sources, and `Rules` which evaluate that data and generate a certain output. `HealthChecks` are essentially wrappers for `DataCollectors` and `Rules`.

### Simple example:

```csharp
// Provide a name and interval (in seconds)
var healthCheck = new HealthCheck("Foo", 5);

// Add a DataCollector (a task that returns data)
healthCheck.AddDataCollector(() => Task.FromResult("Bar" + new Random.Next().ToString()));

// Add a Rule (a predicate and an action)
healthCheck.AddRule<string>(rule => rule
    // First the predicate
    .WhenResult(result => result.Data.Contains("42"),
    // Then the mutation on the output (HealthCheckResult) if predicate evaluates to true
    (output, result) => output.SetStatus(Status.Okay),
    // Then the mutation if it evaluates to false (optional)
    (output, result) => output.SetStatus(Status.Warn).SetReason("Random string does not contain 42")));

```

And that's pretty much the gist of it. Of course it's possible (and recommended) to create a class per `Rule` and `DataCollector` and have them inherit from the base classes. The code above could then become something like this:

```csharp
var healthCheck = new HealthCheck("Foo", 5) 
    .AddDataCollector(new RandomStringGenerator())
    .AddDataCollector(new SomeOtherCollector())
    .AddRule(new ShouldContain42Rule())
    .AddRule(new AndAnotherRule());
```

Small and simple pieces of encapsulated logic can be composed into larger, more complex ones.

The `HealthCheck` will loop over the results of each `DataCollector` and pass those to each `Rule`. It is up to the `Rule` to decide whether it should handle certain data or not.

### Alternative approaches

Both rules and data collectors can be either subclassed or inlined. Inline can be standalone like so:

```csharp
new HealthCheckRule<SomeClass>("SomeRule")
    .WhenResult(dataResult => /*predicate*/,
                (healthCheckResult, dataResult) => /*action if true*/,
                (healthCheckResult, dataResult) => /*(optional) action if false*/);

new DataCollector<SomeClass>(async () => await /*data retrieval logic*/ });
```

Or as an expression on a seeded instance from a HealthCheck:

```csharp
healthCheck.AddRule<SomeClass>(rule => rule.WhenResult(/*...*/))

healthCheck.AddDataCollector<SomeClass>(async () => /*...*/);
```

The rules in `Nimator.CouchBase.Rules` should give you a decent idea of how subclassing works.


# HealthMonitor

The `HealthMonitor` is the main entry point for.. monitoring health. It keeps a list of `HealthChecks` that it loops over with each tick.

Checks will run on a background thread (just like the monitor) and remain completely asynchronous until they are done, at which point they're synchronized again.

A key difference with the original Nimator is the approach to looping and threading: HealthChecks have individual intervals (as do DataCollectors), and HealthMonitor simply ticks every second. It is the responsibility of HealthChecks to indicate (via `bool NeedsToRun`) whether it is time for them to run, and the monitor will call them on the next tick.

```csharp
// Add a check
HealthMonitor.AddCheck(healthCheck);

// Add a basic notifier
HealthMonitor.AddNotifier(ConsoleNotifierSettings.Create().ToNotifier());

// Start ticking
HealthMonitor.StartTicking();

// The main thread will continue while HealthMonitor ticks on a background thread
Console.ReadKey();

// Stop ticking again
HealthMonitor.StopTicking();
```


## EventAggregator

An important (but not very visible) component working in the background is the EventAggregator. You typically don't need to interact with it directly, but it's worth pointing out that this is the component tying together the outputs of `HealthChecks` and passing them to `Notifiers`.

It removes the need for HealthChecks and ultimately the HealthMonitor to pass result data all the way back to the main thread. The data flow stops inside the HealthChecks: they publish the results to the EventAggregator, and Notifiers subscribe to those channels via the HealthMonitor's `AddNotifier`. HealthMonitor never sees any data; it only knows about tasks and subscriptions.


# CouchBase

With all this in place, the code specific to CouchBase ends up being fairly succinct. While there is still work to be done on the side of configuration (managing endpoints and credentials), creating CouchBase HealthChecks boils down to defining rules like so:

```csharp
// Create a warning when there are more than X documents in any given bucket (other examples
// are in Nimator.ConsoleHost).
public sealed class MaxTotalDocumentsInBucket : BucketsRule
{
    public MaxTotalDocumentsInBucket(Identity checkId, long maxDocCount) : base(checkId)
    {
        WhenBucket(
            predicate: bucket => bucket.BasicStats.ItemCount > maxDocCount,
            actionIfTrue: (health, bucket) =>
            {
                health
                    .SetStatus(Status.Warning)
                    .SetLevel(LogLevel.Warn)
                    .SetReason($"Bucket contains {bucket.BasicStats.ItemCount} items (threshold: {maxDocCount}).");
            },
            actionIfFalse: ApplyStandardOkayOperationalPolicy);
    }
}
```

And wiring it up like so (as can be seen in `Nimator.ConsoleHost`):

```csharp
var couchBaseCheck = new ClusterHealthCheck(
    ClusterManagerFactory.FromAppSettings(AppSettings.FromConfigurationManager()));

couchBaseCheck.AddRule(new MaxTotalDocumentsInBucket(couchBaseCheck.Id, 5));

HealthMonitor.AddCheck(couchBaseCheck);
HealthMonitor.AddNotifier(LibLogNotifierSettings.Create().ToNotifier());
HealthMonitor.StartTicking();
```


# Web app

The library is accompanied by a very minimalistic web client which shows a live stream of HealthCheckResults in the browser via SignalR. It's a quick-and-dirty solution purely for demonstration purposes. 

In order to run it, in Visual Studio:
- Right-click the Solution in the Solution Explorer
- Open Properties
- Go to the "Startup Project" tab if you're not already there
- Change the option from "Single startup project" to "Multiple startup projects"
- Select Action "Start" for these two projects:
    - Nimator.Web.Api.ConsoleHost
    - Nimator.Web.Client.ConsoleHost
- Hit "Ok", and then run the solution (like with the ConsoleHost, just "continue" if VS breaks on an exception)
- Open the browser and go to http://localhost:8086

You should see a simple table that outputs health check results as they come in.