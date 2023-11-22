﻿using System;
using System.Diagnostics;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Generic Uniquely identifiable message
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Id} / {Command} / {Parameter}")]
  public struct PipeMessage : IPipeMessage
  {
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <inheritdoc/>
    public string Command { get; set; }

    /// <inheritdoc/>
    public object? Parameter { get; set; }

    /// <summary>
    /// For convience returns the same thing as debugger display
    /// </summary>
    /// <returns>debugger display string</returns>
    public override string ToString() => $"{Id} / {Command} / {Parameter}";
  }
}
