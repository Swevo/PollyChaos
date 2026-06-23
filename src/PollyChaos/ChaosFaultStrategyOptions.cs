// <copyright file="ChaosFaultStrategyOptions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// Options for configuring the chaos fault-injection strategy.
/// </summary>
public sealed class ChaosFaultStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChaosFaultStrategyOptions"/>.
    /// </summary>
    public ChaosFaultStrategyOptions() => Name = "ChaosFault";

    /// <summary>
    /// The probability (0.0–1.0) that a fault is injected on any given call.
    /// Defaults to <c>0.1</c> (10%).
    /// </summary>
    public double InjectionRate { get; set; } = 0.1;

    /// <summary>
    /// A factory that produces the exception to throw when a fault is injected.
    /// Defaults to throwing <see cref="ChaosException"/> when not set.
    /// </summary>
    public Func<Exception>? FaultFactory { get; set; }

    /// <summary>
    /// Whether fault injection is active. Set to <c>false</c> to disable chaos
    /// without removing the strategy from the pipeline.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// An optional callback invoked just before a fault is injected.
    /// </summary>
    public Func<ChaosInjectedArguments, ValueTask>? OnFaultInjected { get; set; }
}
