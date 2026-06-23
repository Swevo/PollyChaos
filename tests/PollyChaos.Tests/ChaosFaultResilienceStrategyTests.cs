// <copyright file="ChaosFaultResilienceStrategyTests.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

using FluentAssertions;
using PollyChaos;

namespace PollyChaos.Tests;

[TestFixture]
public class ChaosFaultResilienceStrategyTests
{
    [Test]
    public async Task FaultNotInjected_ExecutesNormally()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(injectionRate: 0.0)
            .Build();

        var result = await pipeline.ExecuteAsync(async _ =>
        {
            await Task.Yield();
            return 42;
        });

        result.Should().Be(42);
    }

    [Test]
    public async Task FaultAlwaysInjected_ThrowsChaosException()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(injectionRate: 1.0)
            .Build();

        var act = () => pipeline.ExecuteAsync(_ => ValueTask.FromResult(0)).AsTask();

        await act.Should().ThrowAsync<ChaosException>();
    }

    [Test]
    public async Task CustomFault_ThrowsCustomException()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(injectionRate: 1.0, fault: new InvalidOperationException("injected"))
            .Build();

        var act = () => pipeline.ExecuteAsync(_ => ValueTask.FromResult(0)).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("injected");
    }

    [Test]
    public async Task Disabled_NeverInjectsFault()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = 1.0,
                Enabled = false,
            })
            .Build();

        var result = await pipeline.ExecuteAsync(_ => ValueTask.FromResult(99));

        result.Should().Be(99);
    }

    [Test]
    public async Task OnFaultInjected_IsInvokedWhenFaultInjected()
    {
        var callbackInvoked = false;

        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = 1.0,
                OnFaultInjected = _ =>
                {
                    callbackInvoked = true;
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

        try { await pipeline.ExecuteAsync(_ => ValueTask.FromResult(0)); } catch { }

        callbackInvoked.Should().BeTrue();
    }

    [Test]
    public async Task GenericBuilder_FaultAlwaysInjected_ThrowsChaosException()
    {
        var pipeline = new ResiliencePipelineBuilder<int>()
            .AddChaosFault<int>(injectionRate: 1.0)
            .Build();

        var act = () => pipeline.ExecuteAsync(_ => ValueTask.FromResult(0)).AsTask();

        await act.Should().ThrowAsync<ChaosException>();
    }

    [Test]
    public void InvalidInjectionRate_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddChaosFault(injectionRate: 1.5)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NullOptions_Throws()
    {
        var act = () => new ResiliencePipelineBuilder()
            .AddChaosFault((ChaosFaultStrategyOptions)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public async Task FaultFactory_IsCalledPerInjection()
    {
        var callCount = 0;

        var pipeline = new ResiliencePipelineBuilder()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = 1.0,
                FaultFactory = () =>
                {
                    callCount++;
                    return new InvalidOperationException($"fault #{callCount}");
                },
            })
            .Build();

        for (var i = 0; i < 3; i++)
            try { await pipeline.ExecuteAsync(_ => ValueTask.FromResult(0)); } catch { }

        callCount.Should().Be(3);
    }
}
