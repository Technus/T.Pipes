using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Packet
  /// </summary>
  public interface IPipeMessage
  {
    /// <summary>
    /// Marks if this is response
    /// </summary>
    public PacketType PacketType { get; set; }

    /// <summary>
    /// Unique transaction ID
    /// </summary>
    public long Id { get; set; }

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
