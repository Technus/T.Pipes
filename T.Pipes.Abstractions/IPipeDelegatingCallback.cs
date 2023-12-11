using System;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Callback group for <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeDelegatingCallback<in TMessage> : IPipeCallback<TMessage>
  {
    /// <summary>
    /// Client will point to the target implementation<br/>
    /// Server will point to the proxy callback which is 'this'
    /// </summary>
    public IDisposable Target { get; }

    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    public int ResponseTimeoutMs { get; set; }
  }
}
