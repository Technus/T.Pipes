﻿using H.Pipes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServerCallback
    : SpawningPipeCallback<H.Pipes.PipeServer<PipeMessage>>
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

  /// <inheritdoc/>
  public abstract class SpawningPipeClientCallback
    : SpawningPipeCallback<H.Pipes.PipeClient<PipeMessage>>
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
  public abstract class SpawningPipeCallback<TPipe>
    : BaseClass, IPipeCallback<PipeMessage>
    where TPipe : H.Pipes.IPipeConnection<PipeMessage>
  {
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = [];

    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="timeoutMs"></param>
    protected SpawningPipeCallback(TPipe pipe, int timeoutMs = Timeout.Infinite)
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
    protected override void DisposeCore(bool includeAsync)
    {
      if (includeAsync)
      {
        _semaphore.Wait();
        foreach (var server in _mapping.Values)
        {
          server.Dispose();
        }
      }
      _mapping.Clear();
      _semaphore.Dispose();
    }

    /// <summary>
    /// Disposes Proxies
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore()
    {
      await _semaphore.WaitAsync().ConfigureAwait(false);
      foreach (var server in _mapping.Values)
      {
        await server.DisposeAsync().ConfigureAwait(false);
      }
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
    /// Handles creation of Proxies
    /// Calls <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/> with broad generic type
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual void OnMessageReceived(PipeMessage message)
    {
      var name = message.Parameter as string;
      if (name is null || name == string.Empty)
      {
        throw new InvalidOperationException("Name was not specified.");
      }
      _ = ProvideProxyAsyncCore<IPipeDelegatingConnection<PipeMessage>>(message.Command, name);
    }

    /// <summary>
    /// Calls <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/>
    /// Will attempt to cast result to <typeparamref name="T"/>
    /// Use it to expose ready instances
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<T> ProvideProxyAsync<T>(string command, CancellationToken cancellationToken = default)
      where T : IPipeDelegatingConnection<PipeMessage>
      => ProvideProxyAsyncCore<T>(command, string.Empty, cancellationToken);

    /// <summary>
    /// Calls <see cref="CreateProxy(string, string)"/> and initializes it
    /// Will attempt to cast result to <typeparamref name="T"/>
    /// Use it to expose ready instances
    /// </summary>
    /// <param name="command"></param>
    /// <param name="pipeName">name for the pipe, leave empty when making a request</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<T> ProvideProxyAsyncCore<T>(string command, string pipeName = "", CancellationToken cancellationToken = default) 
      where T : IPipeDelegatingConnection<PipeMessage>
    {
      var primaryRequest = pipeName is null || pipeName == string.Empty;
      if(primaryRequest)
        pipeName = Guid.NewGuid().ToString();
      var proxy = (T)CreateProxy(command, pipeName!);
      try
      {
        if(primaryRequest)
          await Pipe.WriteAsync(PipeMessageFactory.Instance.Create(command, pipeName)).ConfigureAwait(false);
        try
        {
          await proxy.StartAndConnectWithTimeoutAsync(ResponseTimeoutMs, cancellationToken).ConfigureAwait(false);
          await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
          _mapping.Add(proxy.PipeName, proxy);
          _semaphore.Release();
          return proxy;
        }
        catch (Exception e)
        {
          if(primaryRequest)
            throw new InvalidOperationException("Command was sent, yet no response.", e);
          else 
            throw;
        }
      }
      catch
      {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        _mapping.Remove(proxy.PipeName);
        _semaphore.Release();
        await proxy.DisposeAsync().ConfigureAwait(false);
        proxy.Callback.Target.Dispose();
        throw;
      }
    }

    /// <summary>
    /// Create instances of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// It will be later cast to final type or the base interface by <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/>
    /// It will be later initialized by <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="pipeName">name for the pipe</param>
    /// <returns></returns>
    protected abstract IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName);
  }
}
