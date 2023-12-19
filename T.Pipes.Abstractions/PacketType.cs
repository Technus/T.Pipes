using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Describes packet content
  /// </summary>
  [Flags]
#pragma warning disable S1939 // Inheritance list should not be redundant
  public enum PacketType : int
#pragma warning restore S1939 // Inheritance list should not be redundant
  {
    /// <summary>
    /// Unknown/default
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Command
    /// </summary>
    Command = 0x01,
    /// <summary>
    /// Response
    /// </summary>
    Response = 0x02,
    /// <summary>
    /// Response
    /// </summary>
    Cancellation = 0x04,
    /// <summary>
    /// Containing exception instead
    /// </summary>
    Failure = 0x08,
    /// <summary>
    /// Command to throw
    /// </summary>
    CommandFailure = Command | Failure,
    /// <summary>
    /// Response to throw
    /// </summary>
    ResponseFailure = Response | Failure,
    /// <summary>
    /// Command to cancel
    /// </summary>
    CommandCancellation = Command | Cancellation,
    /// <summary>
    /// Response to cancel
    /// </summary>
    ResponseCancellation = Response | Cancellation,
  }
}
