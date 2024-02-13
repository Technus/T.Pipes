using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Callback group for <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeDelegatingCallback<TMessage> : IPipeCallback<TMessage>
  {
    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    int ResponseTimeoutMs { get; set; }
  }
}
