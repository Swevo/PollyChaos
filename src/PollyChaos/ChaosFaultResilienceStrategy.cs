// <copyright file="ChaosFaultResilienceStrategy.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// A Polly v8 resilience strategy that injects faults (exceptions) at a configurable rate.
/// </summary>
internal sealed class ChaosFaultResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly ChaosFaultStrategyOptions _options;

    internal ChaosFaultResilienceStrategy(ChaosFaultStrategyOptions options)
    {
        if (options.InjectionRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(options), "InjectionRate must be between 0.0 and 1.0.");

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
            if (_options.OnFaultInjected is not null)
                await _options.OnFaultInjected(new ChaosInjectedArguments(context)).ConfigureAwait(false);

            var fault = _options.FaultFactory?.Invoke() ?? new ChaosException();
            return Outcome.FromException<TResult>(fault);
        }

        return await callback(context, state).ConfigureAwait(false);
    }
}
