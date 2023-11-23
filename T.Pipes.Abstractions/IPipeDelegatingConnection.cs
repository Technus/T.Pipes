using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Abstraction for Client and Server, invokes <see cref="IPipeDelegatingCallback{TMessage}"/> on events.
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeDelegatingConnection<TMessage> : IPipeConnection<TMessage>
  {
    /// <summary>
    /// The Callback
    /// </summary>
    new IPipeDelegatingCallback<TMessage> Callback { get; }
  }
}
