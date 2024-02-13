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
    PacketType PacketType { get; set; }

    /// <summary>
    /// Unique transaction ID
    /// </summary>
    long Id { get; set; }

    /// <summary>
    /// Command to execute
    /// </summary>
    string Command { get; set; }

    /// <summary>
    /// Command parameter
    /// </summary>
    object? Parameter { get; set; }
  }
}
