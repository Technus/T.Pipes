using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Packet
  /// </summary>
  public interface IPipeMessage
  {
    /// <summary>
    /// Unique transaction identificator
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Command to execute
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Command parameter
    /// </summary>
    public object? Parameter { get; set; }
  }
}
