// <copyright file="ChaosLatencyResilienceStrategy.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// A Polly v8 resilience strategy that injects artificial latency at a configurable rate.
/// </summary>
internal sealed class ChaosLatencyResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly ChaosLatencyStrategyOptions _options;

    internal ChaosLatencyResilienceStrategy(ChaosLatencyStrategyOptions options)
    {
        if (options.InjectionRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(options), "InjectionRate must be between 0.0 and 1.0.");
        if (options.Latency < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Latency must not be negative.");

        _options = options;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (_options.Enabled && Random.Shared.NextDouble() < _options.InjectionRate)
        {
            if (_options.OnLatencyInjected is not null)
                await _options.OnLatencyInjected(new ChaosInjectedArguments(context)).ConfigureAwait(false);

            await Task.Delay(_options.Latency, context.CancellationToken).ConfigureAwait(false);
        }

        return await callback(context, state).ConfigureAwait(false);
    }
}
