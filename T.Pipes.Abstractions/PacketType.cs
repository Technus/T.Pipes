using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Describes packet content
  /// </summary>
  [Flags]
  public enum PacketType : int
  {
    /// <summary>
    /// Unknown/default
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Unknown/default
    /// </summary>
    Undefined = 0x00,
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
