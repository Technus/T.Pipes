using System;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Callback group for <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeDelegatingCallback<TMessage> : IPipeCallback<TMessage>
  {
    /// <summary>
    /// Use to check if connection was established correctly the first time
    /// </summary>
    public Task ConnectedOnce { get; }

    /// <summary>
    /// Use to check if connection was failed at least once
    /// </summary>
    public Task FailedOnce { get; }

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
