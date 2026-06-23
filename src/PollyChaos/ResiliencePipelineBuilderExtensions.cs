// <copyright file="ResiliencePipelineBuilderExtensions.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// Extension methods for adding chaos strategies to a <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class ResiliencePipelineBuilderExtensions
{
    // ── Fault injection ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds a chaos fault-injection strategy using the supplied options.
    /// </summary>
    public static ResiliencePipelineBuilder AddChaosFault(
        this ResiliencePipelineBuilder builder,
        ChaosFaultStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.AddStrategy(_ => new ChaosFaultResilienceStrategy<object>(options), options);
    }

    /// <summary>
    /// Adds a chaos fault-injection strategy using the supplied options.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        ChaosFaultStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.AddStrategy(_ => new ChaosFaultResilienceStrategy<TResult>(options), options);
    }

    /// <summary>
    /// Adds a chaos fault-injection strategy that throws <see cref="ChaosException"/> at the given rate.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="injectionRate">Probability (0.0–1.0) of injecting a fault on each call.</param>
    /// <param name="fault">
    /// The exception to throw. Defaults to <see cref="ChaosException"/> when <c>null</c>.
    /// </param>
    public static ResiliencePipelineBuilder AddChaosFault(
        this ResiliencePipelineBuilder builder,
        double injectionRate,
        Exception? fault = null) =>
        builder.AddChaosFault(new ChaosFaultStrategyOptions
        {
            InjectionRate = injectionRate,
            FaultFactory = fault is null ? null : () => fault,
        });

    /// <summary>
    /// Adds a chaos fault-injection strategy that throws <see cref="ChaosException"/> at the given rate.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="injectionRate">Probability (0.0–1.0) of injecting a fault on each call.</param>
    /// <param name="fault">
    /// The exception to throw. Defaults to <see cref="ChaosException"/> when <c>null</c>.
    /// </param>
    public static ResiliencePipelineBuilder<TResult> AddChaosFault<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        double injectionRate,
        Exception? fault = null) =>
        builder.AddChaosFault(new ChaosFaultStrategyOptions
        {
            InjectionRate = injectionRate,
            FaultFactory = fault is null ? null : () => fault,
        });

    // ── Latency injection ────────────────────────────────────────────────────

    /// <summary>
    /// Adds a chaos latency-injection strategy using the supplied options.
    /// </summary>
    public static ResiliencePipelineBuilder AddChaosLatency(
        this ResiliencePipelineBuilder builder,
        ChaosLatencyStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.AddStrategy(_ => new ChaosLatencyResilienceStrategy<object>(options), options);
    }

    /// <summary>
    /// Adds a chaos latency-injection strategy using the supplied options.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    public static ResiliencePipelineBuilder<TResult> AddChaosLatency<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        ChaosLatencyStrategyOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.AddStrategy(_ => new ChaosLatencyResilienceStrategy<TResult>(options), options);
    }

    /// <summary>
    /// Adds a chaos latency-injection strategy that delays calls at the given rate.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="injectionRate">Probability (0.0–1.0) of injecting latency on each call.</param>
    /// <param name="latency">The delay to inject. Defaults to 1 second.</param>
    public static ResiliencePipelineBuilder AddChaosLatency(
        this ResiliencePipelineBuilder builder,
        double injectionRate,
        TimeSpan? latency = null) =>
        builder.AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            InjectionRate = injectionRate,
            Latency = latency ?? TimeSpan.FromSeconds(1),
        });

    /// <summary>
    /// Adds a chaos latency-injection strategy that delays calls at the given rate.
    /// </summary>
    /// <typeparam name="TResult">The result type produced by the pipeline.</typeparam>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="injectionRate">Probability (0.0–1.0) of injecting latency on each call.</param>
    /// <param name="latency">The delay to inject. Defaults to 1 second.</param>
    public static ResiliencePipelineBuilder<TResult> AddChaosLatency<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        double injectionRate,
        TimeSpan? latency = null) =>
        builder.AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            InjectionRate = injectionRate,
            Latency = latency ?? TimeSpan.FromSeconds(1),
        });
}
