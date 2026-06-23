// <copyright file="ChaosException.cs" company="Justin Bannister">
// Copyright (c) Justin Bannister. All rights reserved.
// </copyright>

namespace PollyChaos;

/// <summary>
/// Exception thrown by the chaos fault strategy when no custom
/// <see cref="ChaosFaultStrategyOptions.FaultFactory"/> is provided.
/// </summary>
public sealed class ChaosException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChaosException"/>.
    /// </summary>
    public ChaosException()
        : base("A chaos fault was injected by PollyChaos.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ChaosException"/> with a custom message.
    /// </summary>
    public ChaosException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ChaosException"/> with a message and inner exception.
    /// </summary>
    public ChaosException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
