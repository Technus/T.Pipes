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
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = [];

    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeServerCallback(TPipe pipe, int timeoutMs = Timeout.Infinite)
    {
      Pipe = pipe;
      ResponseTimeoutMs = timeoutMs;
    }

    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    public int ResponseTimeoutMs { get; set; }

    /// <summary>
    /// Used to access data tunnel
    /// </summary>
    public TPipe Pipe { get; }

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
    public virtual async ValueTask DisposeAsync()
    {
      await _semaphore.WaitAsync().ConfigureAwait(false);
      foreach (var server in _mapping.Values)
      {
        server.Dispose();
      }
      _mapping.Clear();
      _semaphore.Dispose();
    }

    /// <summary>
    /// See <see cref="DisposeAsync"/>
    /// </summary>
    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Writes to the pipe directly and calls the Callback On Write
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>do not write to the pipe directly, use that instead, (or the Wrapping Client/Server)</remarks>
    public async Task WriteAsync(PipeMessage message, CancellationToken cancellationToken = default)
    {
      await Pipe.WriteAsync(message, cancellationToken).ConfigureAwait(false);
      OnMessageSent(message);
    }


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public virtual void Connected(string connection) => Clear();


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public virtual void Disconnected(string connection) => Clear();


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public virtual void OnExceptionOccurred(Exception e) => Clear();

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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task<T> RequestProxyAsync<T>(string command, T implementationServer, CancellationToken cancellationToken = default)
      where T : IPipeDelegatingConnection<PipeMessage>
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      if(ResponseTimeoutMs>=0)
        cts.CancelAfter(ResponseTimeoutMs);

      try
      {
        await implementationServer.StartAsync(cts.Token).ConfigureAwait(false);
        await WriteAsync(PipeMessageFactory.Instance.Create(command, implementationServer.PipeName), cancellationToken).ConfigureAwait(false);
      }
      catch
      {
        await implementationServer.DisposeAsync();
        throw;
      }
      finally
      {

      }

      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(connectedTask, Task.Delay(ResponseTimeoutMs)).ConfigureAwait(false) == connectedTask && connectedTask.IsCompletedSuccessfully())
      {
        cts.Cancel();
        _semaphore.Wait();
        _mapping.Add(implementationServer.PipeName, implementationServer);
        _semaphore.Release();
        return implementationServer;
      }
      await implementationServer.DisposeAsync().ConfigureAwait(false);
      throw new InvalidOperationException($"The {nameof(command)}: {command}, could not be performed, connection was not started or connection was impossible.");
    }
  }
}
