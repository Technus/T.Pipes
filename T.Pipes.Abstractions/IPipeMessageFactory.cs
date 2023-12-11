using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Creates Packets
  /// </summary>
  /// <typeparam name="T">packet type</typeparam>
  public interface IPipeMessageFactory<T> where T : IPipeMessage
  {
    /// <summary>
    /// Create packet without parameter
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <returns>command packet</returns>
    T CreateCommand(string command);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns>command packet</returns>
    T CreateCommand(string command, object? parameter);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns>command packet</returns>
    T CreateCommandCancellation(string command, Exception? parameter = default);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns>command packet</returns>
    T CreateCommandFailure(string command, Exception parameter);

    /// <summary>
    /// Create packet without parameter
    /// </summary>
    /// <param name="commandMessage">packet to respond to</param>
    /// <returns>response packet</returns>
    T CreateResponse(T commandMessage);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="commandMessage">packet to respond to</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns></returns>
    T CreateResponse(T commandMessage, object? parameter);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="commandMessage">packet to respond to</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns></returns>
    T CreateResponseCancellation(T commandMessage, Exception? parameter = default);

    /// <summary>
    /// Create packet with parameter
    /// </summary>
    /// <param name="commandMessage">packet to respond to</param>
    /// <param name="parameter">parameter to pass along</param>
    /// <returns></returns>
    T CreateResponseFailure(T commandMessage, Exception parameter);
  }
}
