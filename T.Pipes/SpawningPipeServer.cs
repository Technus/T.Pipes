using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServer<TCallback> : SpawningPipeServer<H.Pipes.PipeServer<PipeMessage>, TCallback>
    where TCallback : SpawningPipeServerCallback<H.Pipes.PipeServer<PipeMessage>>
  {
    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="clientExeName"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(string pipe, string clientExeName, TCallback callback) : base(new(pipe), clientExeName, callback)
    {
    }

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="clientExeName"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, string clientExeName, TCallback callback) : base(pipe, clientExeName, callback)
    {
    }
  }


  /// <summary>
  /// Helper to wrap factorization of <see cref="T.Pipes.Abstractions.IPipeDelegatingConnection{TMessage}"/> Servers
  /// implements methods calling <see cref="SpawningPipeServerCallback{TPipe}.RequestProxyAsync{T}(string, T)"/> to provide proxies
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class SpawningPipeServer<TPipe, TCallback> : PipeServer<TPipe, PipeMessage, TCallback>
    where TCallback : SpawningPipeServerCallback<TPipe>
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
  {
    private readonly Process _process;

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="clientExeName"></param>
    /// <param name="timeoutMs"></param>
    /// <param name="callback"></param>
    public SpawningPipeServer(TPipe pipe, string clientExeName, TCallback callback) : base(pipe, callback) 
      => _process = new Process { StartInfo = new(clientExeName) };

    /// <summary>
    /// Disposes Pipe, Callback and Client Process
    /// </summary>
    /// <returns></returns>
    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
      _process.Close();
      _process.Dispose();
    }

    /// <summary>
    /// Starts the Pipe, attempts connection and throws on failure
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">when a connection was not possible</exception>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      var startTask = base.StartAsync(cancellationToken);
      await startTask;
      if(startTask.IsCompleted)
      {
        _process.Start();
        var connectedTask = Callback.ConnectedOnce;
        using var cts = new CancellationTokenSource();
        if (await Task.WhenAny(connectedTask, Task.Delay(Callback.ResponseTimeoutMs, cts.Token)) == connectedTask && connectedTask.IsCompleted)
        {
          cts.Cancel();
          return;
        }
        _process.Close();
        await StopAsync(CancellationToken.None);
      }
      throw new InvalidOperationException($"Either the client was not started or connection was impossible");
    }
  }
}
