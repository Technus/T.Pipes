using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServerCallback
    : SpawningPipeServerCallback<H.Pipes.PipeServer<PipeMessage>>
  {
    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe, int timeoutMs) : base(pipe, timeoutMs)
    {
    }
  }

  /// <summary>
  /// Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  public abstract class SpawningPipeServerCallback<TPipe> 
    : IPipeCallback<PipeMessage>
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = [];

    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeServerCallback(TPipe pipe, int timeoutMs)
    {
      Pipe = pipe;
      ResponseTimeoutMs = timeoutMs;
    }

    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    public int ResponseTimeoutMs { get; }

    /// <summary>
    /// Used to access data tunnel
    /// </summary>
    public TPipe Pipe { get; }

    /// <summary>
    /// Task that checks if a single connection was achieved
    /// </summary>
    public Task ConnectedOnce => _connectedOnce.Task;

    /// <summary>
    /// Disposes Proxies
    /// </summary>
    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (var server in _mapping.Values)
      {
        server.Dispose();
      }
      _mapping.Clear();
      _semaphore.Release();
    }

    /// <summary>
    /// Disposes Proxies
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask DisposeAsync()
    {
      _connectedOnce.TrySetCanceled();
      _semaphore.Wait();
      foreach (var server in _mapping.Values)
      {
        server.Dispose();
      }
      _mapping.Clear();
      _semaphore.Dispose();
      return default;
    }

    /// <summary>
    /// See <see cref="DisposeAsync"/>
    /// </summary>
    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Writes to the pipe directly and calls the Callback On Write
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <remarks>do not write to the pipe directly, use that instead, (or the Wrapping Client/Server)</remarks>
    public void Write(PipeMessage message)
    {
      OnMessageSent(message);
      _ = Pipe.WriteAsync(message);
    }


    /// <summary>
    /// disposes created Proxies, and sets <see cref="ConnectedOnce"/> result
    /// </summary>
    public virtual void Connected(string connection)
    {
      Clear();
      _connectedOnce.TrySetResult(null);
    }


    /// <summary>
    /// disposes created Proxies, and sets <see cref="ConnectedOnce"/> result
    /// </summary>
    public virtual void Disconnected(string connection)
    {
      Clear();
      _connectedOnce.TrySetCanceled();
    }


    /// <summary>
    /// disposes created Proxies, and sets <see cref="ConnectedOnce"/> result
    /// </summary>
    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear();
      _connectedOnce.TrySetException(e);
    }

    /// <summary>
    /// No-op handling of sent messages
    /// </summary>
    /// <param name="message"></param>
    public virtual void OnMessageSent(PipeMessage message) { }

    /// <summary>
    /// No-op handling of sent messages
    /// </summary>
    /// <param name="message"></param>
    public virtual void OnMessageReceived(PipeMessage message) { }

    /// <summary>
    /// Helper to expose creation of objects trough this Pipe.<br/>
    /// Just make public methods calling this one.<br/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <param name="implementationServer"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task<T> RequestProxyAsync<T>(string command, T implementationServer)
      where T : IPipeDelegatingConnection<PipeMessage>
    {
      var failedOnce = implementationServer.Callback.FailedOnce;

      _ = failedOnce.ContinueWith(async x =>
      {
        await _semaphore.WaitAsync();
        _mapping.Remove(implementationServer.ServerName);
        _semaphore.Release();
        _ = implementationServer.DisposeAsync();
      }, TaskContinuationOptions.OnlyOnRanToCompletion);

      _ = failedOnce.ContinueWith(async x => {
        await _semaphore.WaitAsync();
        _mapping.Remove(implementationServer.ServerName);
        _semaphore.Release();
      }, TaskContinuationOptions.OnlyOnCanceled);

      var startTask = implementationServer.StartAsync();
      await startTask;
      if (startTask.IsCompleted)
      {
        Write(PipeMessageFactory.Instance.Create(command, implementationServer.ServerName));
        var connectedTask = implementationServer.Callback.ConnectedOnce;
        using var cts = new CancellationTokenSource();
        if (await Task.WhenAny(connectedTask, Task.Delay(ResponseTimeoutMs)) == connectedTask && connectedTask.IsCompleted)
        {
          cts.Cancel();
          _semaphore.Wait();
          _mapping.Add(implementationServer.ServerName, implementationServer);
          _semaphore.Release();
          return implementationServer;
        }
      }
      _ = implementationServer.DisposeAsync();
      throw new InvalidOperationException($"The {nameof(command)}: {command}, could not be performed, connection was not started or connection was impossible.");
    }
  }
}
