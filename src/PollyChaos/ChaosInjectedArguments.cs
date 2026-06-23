// <copyright file="ChaosInjectedArguments.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// Arguments passed to chaos callbacks when an injection occurs.
/// </summary>
/// <param name="Context">The resilience context for the current call.</param>
public readonly record struct ChaosInjectedArguments(ResilienceContext Context);
