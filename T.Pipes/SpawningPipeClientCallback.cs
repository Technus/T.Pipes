using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeClientCallback
    : SpawningPipeClientCallback<H.Pipes.PipeClient<PipeMessage>>
  {
    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, int timeoutMs) : base(pipe, timeoutMs)
    {
    }
  }

  /// <summary>
  /// Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  public abstract class SpawningPipeClientCallback<TPipe>
    : IPipeCallback<PipeMessage>
    where TPipe : H.Pipes.IPipeClient<PipeMessage>
  {
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = [];

    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeClientCallback(TPipe pipe, int timeoutMs = Timeout.Infinite)
    {
      ResponseTimeoutMs = timeoutMs;
      Pipe = pipe;
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
    /// On connected disposes created Proxies
    /// </summary>
    /// <param name="connection"></param>
    public virtual void Connected(string connection) => Clear();

    /// <summary>
    /// On disconnected disposes created Proxies
    /// </summary>
    /// <param name="connection"></param>
    public virtual void Disconnected(string connection)
    {
      Clear();
      throw new InvalidOperationException("Disconnected occurred in client");
    }

    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (var client in _mapping.Values)
      {
        client.Dispose();
      }
      _mapping.Clear();
      _semaphore.Release();
    }

    /// <summary>
    /// See <see cref="DisposeAsync"/>
    /// </summary>
    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Disposes created Proxies
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask DisposeAsync()
    {
      await _semaphore.WaitAsync().ConfigureAwait(false);
      foreach (var client in _mapping.Values)
      {
        client.Dispose();
      }
      _mapping.Clear();
      _semaphore.Dispose();
    }

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
    public virtual void OnExceptionOccurred(Exception e) => Clear();

    /// <summary>
    /// Handles creation of Proxies on both sides and establishing a connection
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual void OnMessageReceived(PipeMessage message)
    {
      var proxy = CreateProxy(message);
      try
      {
        proxy.StartAndConnectWithTimeoutAsync(ResponseTimeoutMs).Wait();
        _semaphore.Wait();
        _mapping.Add(proxy.PipeName, proxy);
        _semaphore.Release();
      }
      catch
      {
        proxy.Dispose();
        proxy.Callback.Target.Dispose();
        throw;
      }
    }

    /// <summary>
    /// Should create instances of <see cref="IPipeDelegatingConnection{TMessage}"/> but nothing else (not starting not registering in IOC etc.)
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public abstract IPipeDelegatingConnection<PipeMessage> CreateProxy(PipeMessage command);

    /// <summary>
    /// No-op handling of sent messages
    /// </summary>
    /// <param name="message"></param>
    public virtual void OnMessageSent(PipeMessage message) { }
  }
}
