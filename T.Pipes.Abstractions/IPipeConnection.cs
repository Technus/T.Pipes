using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Abstraction for Client and Server, invokes <see cref="IPipeCallback{TMessage}"/> on events.
  /// </summary>
  /// <typeparam name="TMessage">packet type <see cref="IPipeMessage"/> implementations are usually used</typeparam>
  public interface IPipeConnection<TMessage> : IAsyncDisposable, IDisposable
  {
    /// <summary>
    /// The Pipe Callback
    /// </summary>
    IPipeCallback<TMessage> Callback { get; }

    /// <summary>
    /// Writes the packet to pipe
    /// </summary>
    /// <param name="value">packet to write</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(TMessage value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the pipe, finishes ~instantly
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts and then awaits incoming connection
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAndConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Allows to start and await connection for a specified time
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAndConnectWithTimeoutAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls <see cref="StartAndConnectWithTimeoutAsync(int, CancellationToken)"/>
    /// And awaits, it will also get cancelled on the <see cref="IPipeCallback{TMessage}.LifetimeCancellation"/>
    /// </summary>
    /// <returns>Only after the client is Cancelled</returns>
    public Task StartAndConnectWithTimeoutAndAwaitCancellationAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the pipe, finishes ~instantly
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// For servers checks if it is running
    /// For clients if it is running and connected or connecting
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// For servers it is <see cref="ServerName"/>
    /// For clients it is the generated unique name
    /// </summary>
    string PipeName { get; }
    
    /// <summary>
    /// The server pipe name
    /// </summary>
    string ServerName { get; }
  }
}
