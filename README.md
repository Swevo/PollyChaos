# PollyChaos

<img src="icon.png" width="100" align="right" />

[![NuGet](https://img.shields.io/nuget/v/PollyChaos.svg)](https://www.nuget.org/packages/PollyChaos)
[![CI](https://github.com/Swevo/PollyChaos/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/PollyChaos/actions/workflows/build.yml)

Chaos engineering and fault-injection resilience strategies for **Polly v8** pipelines. Inject faults, latency at a configurable rate to harden your services against failure.

The spiritual successor to **Polly.Contrib.Simmy** for Polly v8 — lightweight, zero extra dependencies, and fully composable with any Polly pipeline.

## Install

```
dotnet add package PollyChaos
```

## Usage

### Fault injection

Throw a `ChaosException` on 10% of calls:

```csharp
using PollyChaos;

var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.1)
    .Build();
```

Throw a custom exception:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.05, fault: new TimeoutException("chaos timeout"))
    .Build();
```

### Latency injection

Add a 2-second delay on 5% of calls:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosLatency(injectionRate: 0.05, latency: TimeSpan.FromSeconds(2))
    .Build();
```

### Combined chaos pipeline

```csharp
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddChaosFault<HttpResponseMessage>(injectionRate: 0.05)   // 5% faults
    .AddChaosLatency<HttpResponseMessage>(injectionRate: 0.1)  // 10% latency
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
    })
    .Build();
```

### Full options

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(new ChaosFaultStrategyOptions
    {
        InjectionRate = 0.1,
        Enabled = true,  // flip to false to disable without removing from pipeline
        FaultFactory = () => new HttpRequestException("injected network error"),
        OnFaultInjected = args =>
        {
            logger.LogWarning("Chaos fault injected for {Operation}", args.Context.OperationKey);
            return ValueTask.CompletedTask;
        },
    })
    .AddChaosLatency(new ChaosLatencyStrategyOptions
    {
        InjectionRate = 0.1,
        Latency = TimeSpan.FromSeconds(2),
        Enabled = true,
        OnLatencyInjected = args =>
        {
            logger.LogWarning("Chaos latency injected for {Operation}", args.Context.OperationKey);
            return ValueTask.CompletedTask;
        },
    })
    .Build();
```

### Toggle chaos via configuration

Use `Enabled` to flip chaos on/off without rebuilding the pipeline — ideal for feature flags:

```csharp
var chaosOptions = new ChaosFaultStrategyOptions
{
    InjectionRate = 0.1,
    Enabled = config.GetValue<bool>("ChaosEngineering:Enabled"),
};

var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(chaosOptions)
    .Build();
```

## Composition

Place chaos strategies **outside** (before) retry so injected faults are retried:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.1)   // 1. inject faults
    .AddChaosLatency(injectionRate: 0.1) // 2. inject latency
    .AddRetry(...)                        // 3. retry on failure
    .AddCircuitBreaker(...)              // 4. protect downstream
    .Build();
```

## Support

If PollyChaos helps you ship more resilient software, consider supporting the project:

[![Sponsor](https://img.shields.io/badge/Sponsor-%E2%9D%A4-pink?logo=github)](https://github.com/sponsors/Swevo)

> 💼 **Need .NET resilience help?** Visit [solidqualitysolutions.com](https://solidqualitysolutions.com/) for consulting and architecture services.

## License

MIT
