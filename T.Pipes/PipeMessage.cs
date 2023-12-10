using System;
using System.Diagnostics;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Generic Uniquely identifiable message
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Id} / {Command} / {Parameter}")]
  public class PipeMessage : IPipeMessage
  {
    /// <inheritdoc/>
    public long Id { get; set; }

    /// <inheritdoc/>
    public PacketType PacketType { get; set; }

    /// <inheritdoc/>
    public string Command { get; set; } = string.Empty;

    /// <inheritdoc/>
    public object? Parameter { get; set; }

    /// <summary>
    /// For convenience returns the same thing as debugger display
    /// </summary>
    /// <returns>debugger display string</returns>
    public override string ToString() => $"{Id} / {Command} / {PacketType} / {Parameter}";
  }
}
