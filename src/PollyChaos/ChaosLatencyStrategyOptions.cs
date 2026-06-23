// <copyright file="ChaosLatencyStrategyOptions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// Options for configuring the chaos latency-injection strategy.
/// </summary>
public sealed class ChaosLatencyStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChaosLatencyStrategyOptions"/>.
    /// </summary>
    public ChaosLatencyStrategyOptions() => Name = "ChaosLatency";

    /// <summary>
    /// The probability (0.0–1.0) that a latency delay is injected on any given call.
    /// Defaults to <c>0.1</c> (10%).
    /// </summary>
    public double InjectionRate { get; set; } = 0.1;

    /// <summary>
    /// The amount of latency to inject when triggered.
    /// Defaults to <c>1 second</c>.
    /// </summary>
    public TimeSpan Latency { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether latency injection is active. Set to <c>false</c> to disable chaos
    /// without removing the strategy from the pipeline.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// An optional callback invoked just before latency is injected.
    /// </summary>
    public Func<ChaosInjectedArguments, ValueTask>? OnLatencyInjected { get; set; }
}
