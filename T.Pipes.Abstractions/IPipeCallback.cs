﻿using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Callback group for <see cref="IPipeConnection{TMessage}"/>
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeCallback<TMessage> : IDisposable, IAsyncDisposable
  {
    /// <summary>
    /// Triggers when sending
    /// </summary>
    /// <param name="message">the message being sent</param>
    void OnMessageSent(TMessage? message);

    /// <summary>
    /// Triggers on exceptions in the pipe
    /// </summary>
    /// <param name="e">exception that was raised</param>
    void OnExceptionOccurred(Exception e);

    /// <summary>
    /// Triggers when receiving
    /// </summary>
    /// <param name="message"></param>
    void OnMessageReceived(TMessage? message);

    /// <summary>
    /// Triggers when a connection was established
    /// </summary>
    /// <param name="connection">the unique pipe id for this client/server internal pipe</param>
    void Connected(string connection);

    /// <summary>
    /// Triggers when a connection was closed, alternatively on error <see cref="OnExceptionOccurred(Exception)"/>
    /// </summary>
    /// <param name="connection">the unique pipe id for this client/server internal pipe</param>
    void Disconnected(string connection);
  }
}
