# PollyChaos

<img src="icon.png" width="100" align="right" />

[![NuGet](https://img.shields.io/nuget/v/PollyChaos.svg)](https://www.nuget.org/packages/PollyChaos)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PollyChaos.svg)](https://www.nuget.org/packages/PollyChaos)
[![CI](https://github.com/Swevo/PollyChaos/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/PollyChaos/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Chaos engineering for Polly v8.** Inject faults and latency into your resilience pipelines to prove your system handles failures gracefully — before production does it for you.

The **Simmy-compatible** chaos companion for Polly v8. If you used `Polly.Contrib.Simmy` with Polly v7, this is what you have been waiting for.

## Install

```
dotnet add package PollyChaos
```

## Quick start

```csharp
using PollyChaos;

// Throw an exception on 10% of calls
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.1)
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
    .Build();

await pipeline.ExecuteAsync(ct => httpClient.GetAsync("/api/orders", ct), cancellationToken);
```

## Why PollyChaos?

| Feature | Polly.Contrib.Simmy (v7) | PollyChaos (v8) |
|---|---|---|
| Polly version | v7 only | **v8** |
| Fault injection | ✅ | ✅ |
| Latency injection | ✅ | ✅ |
| `Enabled` toggle | ✅ | ✅ |
| Generic pipeline (`Builder<T>`) | ✅ | ✅ |
| Callbacks (`OnFaultInjected`) | ❌ | ✅ |
| .NET 9 support | ❌ | ✅ |
| Zero extra dependencies | ❌ | ✅ |

## Usage

### Fault injection

```csharp
// Throw ChaosException on 10% of calls
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.1)
    .Build();

// Throw a custom exception
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.05, fault: new HttpRequestException("injected failure"))
    .Build();
```

### Latency injection

```csharp
// Add a 2-second delay on 5% of calls
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosLatency(injectionRate: 0.05, latency: TimeSpan.FromSeconds(2))
    .Build();
```

### Combined chaos pipeline

```csharp
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddChaosFault<HttpResponseMessage>(injectionRate: 0.05)   // 5% exceptions
    .AddChaosLatency<HttpResponseMessage>(injectionRate: 0.1)  // 10% slow calls
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage> { MaxRetryAttempts = 3 })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>())
    .Build();
```

### Toggle via configuration (feature flag)

Flip chaos on/off without rebuilding the pipeline — ideal for integration test environments:

```csharp
var chaosOptions = new ChaosFaultStrategyOptions
{
    InjectionRate = 0.2,
    Enabled = config.GetValue<bool>("ChaosEngineering:Enabled"),
    FaultFactory = () => new TimeoutException("chaos: downstream timeout"),
    OnFaultInjected = args =>
    {
        logger.LogWarning("Chaos fault injected for {Operation}", args.Context.OperationKey);
        return ValueTask.CompletedTask;
    },
};

var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(chaosOptions)
    .Build();
```

### ASP.NET Core integration

```csharp
// Program.cs
builder.Services.AddResiliencePipeline("orders-client", (pipelineBuilder, context) =>
{
    var config = context.ServiceProvider.GetRequiredService<IConfiguration>();
    var chaosEnabled = config.GetValue<bool>("ChaosEngineering:Enabled");

    pipelineBuilder
        .AddChaosFault(new ChaosFaultStrategyOptions
        {
            InjectionRate = 0.1,
            Enabled = chaosEnabled,
        })
        .AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            InjectionRate = 0.05,
            Latency = TimeSpan.FromSeconds(3),
            Enabled = chaosEnabled,
        })
        .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions());
});
```

## Pipeline order

Place chaos strategies **outside** (before) retry so injected faults are retried — just like real failures:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddChaosFault(injectionRate: 0.1)   // 1. inject faults
    .AddChaosLatency(injectionRate: 0.1) // 2. inject latency
    .AddRetry(...)                        // 3. retry failures
    .AddCircuitBreaker(...)              // 4. trip if too many failures
    .Build();
```

## Related Packages

| Package | Downloads | Description |
|---|---|---|
| [PollyHealthChecks](https://www.nuget.org/packages/PollyHealthChecks) | [![Downloads](https://img.shields.io/nuget/dt/PollyHealthChecks.svg)](https://www.nuget.org/packages/PollyHealthChecks) | ASP.NET Core health checks for Polly v8 circuit breakers — expose circuit-breaker state (Closed, HalfOpen, Open, Isolated) as /health endpoint responses |
| [PollyOpenTelemetry](https://www.nuget.org/packages/PollyOpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/PollyOpenTelemetry.svg)](https://www.nuget.org/packages/PollyOpenTelemetry) | OpenTelemetry instrumentation for Polly v8 resilience pipelines |
| [PollyBackoff](https://www.nuget.org/packages/PollyBackoff) | [![Downloads](https://img.shields.io/nuget/dt/PollyBackoff.svg)](https://www.nuget.org/packages/PollyBackoff) | Backoff delay strategies for Polly v8 resilience pipelines |
| [PollyGrpc](https://www.nuget.org/packages/PollyGrpc) | [![Downloads](https://img.shields.io/nuget/dt/PollyGrpc.svg)](https://www.nuget.org/packages/PollyGrpc) | Polly v8 resilience interceptor for gRPC |
| [PollyEFCore](https://www.nuget.org/packages/PollyEFCore) | [![Downloads](https://img.shields.io/nuget/dt/PollyEFCore.svg)](https://www.nuget.org/packages/PollyEFCore) | Polly v8 resilience pipelines for Entity Framework Core — wrap every EF Core query and SaveChanges with retry, timeout and circuit-breaker via a single AddPollyResilience() call |
| [PollyMailKit](https://www.nuget.org/packages/PollyMailKit) | [![Downloads](https://img.shields.io/nuget/dt/PollyMailKit.svg)](https://www.nuget.org/packages/PollyMailKit) | Polly v8 resilience pipelines for MailKit — retry, timeout, and circuit-breaker for SmtpClient.SendAsync and any MailKit SMTP operation |
| [PollyOpenAI](https://www.nuget.org/packages/PollyOpenAI) | [![Downloads](https://img.shields.io/nuget/dt/PollyOpenAI.svg)](https://www.nuget.org/packages/PollyOpenAI) | Polly v8 resilience for OpenAI and Azure OpenAI API calls |
| [PollySignalR](https://www.nuget.org/packages/PollySignalR) | [![Downloads](https://img.shields.io/nuget/dt/PollySignalR.svg)](https://www.nuget.org/packages/PollySignalR) | Polly v8 reconnect policy for SignalR |
| [PollyHangfire](https://www.nuget.org/packages/PollyHangfire) | [![Downloads](https://img.shields.io/nuget/dt/PollyHangfire.svg)](https://www.nuget.org/packages/PollyHangfire) | Polly v8 resilience pipelines for Hangfire — retry, timeout, and circuit-breaker for IBackgroundJobClient.Enqueue and Schedule |
| [PollyMediatR](https://www.nuget.org/packages/PollyMediatR) | [![Downloads](https://img.shields.io/nuget/dt/PollyMediatR.svg)](https://www.nuget.org/packages/PollyMediatR) | Polly v8 resilience pipelines for MediatR — add retry, timeout, circuit-breaker, rate-limiting, hedging, and chaos engineering to any MediatR request handler with a single line of DI registration |
| [PollyAzureQueueStorage](https://www.nuget.org/packages/PollyAzureQueueStorage) | [![Downloads](https://img.shields.io/nuget/dt/PollyAzureQueueStorage.svg)](https://www.nuget.org/packages/PollyAzureQueueStorage) | Polly v8 resilience pipelines for Azure Queue Storage — retry, timeout, and circuit-breaker for Azure.Storage.Queues QueueClient |
| [PollyRedis](https://www.nuget.org/packages/PollyRedis) | [![Downloads](https://img.shields.io/nuget/dt/PollyRedis.svg)](https://www.nuget.org/packages/PollyRedis) | Polly v8 resilience for StackExchange.Redis |
| [PollyAzureServiceBus](https://www.nuget.org/packages/PollyAzureServiceBus) | [![Downloads](https://img.shields.io/nuget/dt/PollyAzureServiceBus.svg)](https://www.nuget.org/packages/PollyAzureServiceBus) | Polly v8 resilience for Azure Service Bus — retry, circuit breaker, and timeout for sending and receiving messages |
| [PollyKafka](https://www.nuget.org/packages/PollyKafka) | [![Downloads](https://img.shields.io/nuget/dt/PollyKafka.svg)](https://www.nuget.org/packages/PollyKafka) | Polly v8 resilience for Confluent.Kafka — retry, circuit breaker, and timeout for producers and consumers |
| [PollyRateLimiter](https://www.nuget.org/packages/PollyRateLimiter) | [![Downloads](https://img.shields.io/nuget/dt/PollyRateLimiter.svg)](https://www.nuget.org/packages/PollyRateLimiter) | Convenience extension methods for Polly v8 resilience pipelines: AddFixedWindowRateLimiter, AddSlidingWindowRateLimiter, and AddTokenBucketRateLimiter |
| [PollyCaching](https://www.nuget.org/packages/PollyCaching) | [![Downloads](https://img.shields.io/nuget/dt/PollyCaching.svg)](https://www.nuget.org/packages/PollyCaching) | A caching resilience strategy for Polly v8 pipelines |
| [PollyBulkhead](https://www.nuget.org/packages/PollyBulkhead) | [![Downloads](https://img.shields.io/nuget/dt/PollyBulkhead.svg)](https://www.nuget.org/packages/PollyBulkhead) | Bulkhead isolation strategy for Polly v8 resilience pipelines |

## Support

If PollyChaos helps you ship more resilient software, consider supporting the project:

[![Sponsor](https://img.shields.io/badge/Sponsor-%E2%9D%A4-pink?logo=github)](https://github.com/sponsors/Swevo)

> 💼 **Need .NET resilience help?** Visit [solidqualitysolutions.com](https://solidqualitysolutions.com/) for consulting and architecture services.

| [PollyRabbitMQ](https://www.nuget.org/packages/PollyRabbitMQ) | Polly v8 resilience for RabbitMQ.Client channels |

## Also by the same author

> 🌐 **[swevo.github.io](https://swevo.github.io/)**

| Package | Description |
|---|---|
| [**AutoLog.Generator**](https://github.com/Swevo/AutoLog.Generator) | Compile-time high-performance logging — `[Log(Level, Message)]` generates `LoggerMessage.Define`. AOT-safe. |
| [**AutoHttpClient.Generator**](https://github.com/Swevo/AutoHttpClient.Generator) | Compile-time typed HTTP client — `[HttpClient]` on an interface generates a strongly-typed client. AOT-safe Refit alternative. |
| [**AutoDispatch.Generator**](https://github.com/Swevo/AutoDispatch.Generator) | Compile-time CQRS dispatcher — `[Handler]` generates a strongly-typed `IDispatcher`. MediatR alternative. |
| [**AutoWire**](https://github.com/Swevo/AutoWire) | Compile-time DI auto-registration — `[Scoped]`/`[Singleton]`/`[Transient]` generates `IServiceCollection` registration code. |
| [**AutoMap.Generator**](https://github.com/Swevo/AutoMap.Generator) | Compile-time object mapping — `[Map(typeof(Dto))]` generates `ToDto()` extension methods. AutoMapper alternative. |

## License

MIT
