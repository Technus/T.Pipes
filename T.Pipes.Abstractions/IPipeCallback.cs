using System;
using System.Threading;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Callback group for <see cref="IPipeConnection{TMessage}"/>
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeCallback<TMessage> : IDisposable, IAsyncDisposable
  {
    /// <summary>
    /// Allows to set the Connection this callback was applied to
    /// Setter should allow only one set with non null value
    /// </summary>
    IPipeConnection<TMessage> Connection { get; set; }

    /// <summary>
    /// Cancelled on dispose or finalize just in case, or when should be cancelled or finalized
    /// </summary>
    CancellationToken LifetimeCancellation { get; }

    /// <summary>
    /// Triggers when sending
    /// </summary>
    /// <param name="message">the message being sent</param>
    void OnMessageSent(TMessage message);

    /// <summary>
    /// Triggers on exceptions in the pipe
    /// </summary>
    /// <param name="e">exception that was raised</param>
    void OnExceptionOccurred(Exception e);

    /// <summary>
    /// Triggers when receiving
    /// </summary>
    /// <param name="message"></param>
    /// <remarks>To prevent circular dependencies must also be called inside callback before writing to pure pipe, (not the wrapped pipe)</remarks>
    void OnMessageReceived(TMessage message);

    /// <summary>
    /// Triggers when a connection was established
    /// </summary>
    /// <param name="connection">the unique pipe id for this client/server internal pipe</param>
    void OnConnected(string connection);

    /// <summary>
    /// Triggers when a connection was closed, alternatively on error <see cref="OnExceptionOccurred(Exception)"/>
    /// </summary>
    /// <param name="connection">the unique pipe id for this client/server internal pipe</param>
    void OnDisconnected(string connection);
  }
}
