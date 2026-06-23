// <copyright file="ChaosLatencyResilienceStrategyTests.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

using FluentAssertions;
using PollyChaos;

namespace PollyChaos.Tests;

[TestFixture]
public class ChaosLatencyResilienceStrategyTests
{
    [Test]
    public async Task LatencyNotInjected_ExecutesNormally()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(injectionRate: 0.0, latency: TimeSpan.FromSeconds(10))
            .Build();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await pipeline.ExecuteAsync(_ => ValueTask.FromResult(42));
        sw.Stop();

        result.Should().Be(42);
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Test]
    public async Task LatencyAlwaysInjected_AddsDelay()
    {
        var latency = TimeSpan.FromMilliseconds(100);

        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(injectionRate: 1.0, latency: latency)
            .Build();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(80);
    }

    [Test]
    public async Task Disabled_NeverInjectsLatency()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                InjectionRate = 1.0,
                Latency = TimeSpan.FromSeconds(10),
                Enabled = false,
            })
            .Build();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Test]
    public async Task OnLatencyInjected_IsInvokedWhenLatencyInjected()
    {
        var callbackInvoked = false;

        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                InjectionRate = 1.0,
                Latency = TimeSpan.FromMilliseconds(10),
                OnLatencyInjected = _ =>
                {
                    callbackInvoked = true;
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        callbackInvoked.Should().BeTrue();
    }

    [Test]
    public async Task Cancellation_CancelsInjectedLatency()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosLatency(injectionRate: 1.0, latency: TimeSpan.FromSeconds(10))
            .Build();

        var act = () => pipeline.ExecuteAsync(_ => ValueTask.CompletedTask, cts.Token).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task GenericBuilder_LatencyAlwaysInjected_AddsDelay()
    {
        var pipeline = new ResiliencePipelineBuilder<int>()
            .AddChaosLatency<int>(injectionRate: 1.0, latency: TimeSpan.FromMilliseconds(100))
            .Build();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await pipeline.ExecuteAsync(_ => ValueTask.FromResult(1));
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(80);
    }

    [Test]
    public void InvalidInjectionRate_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddChaosLatency(injectionRate: -0.1)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NegativeLatency_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                InjectionRate = 0.1,
                Latency = TimeSpan.FromSeconds(-1),
            })
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NullOptions_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddChaosLatency((ChaosLatencyStrategyOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
