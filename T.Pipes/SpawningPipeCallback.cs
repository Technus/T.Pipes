﻿using System;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  public abstract class SpawningPipeCallback
    : PipeCallbackBase<PipeMessage, PipeMessageFactory, SpawningPipeCallback>
  {
    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected SpawningPipeCallback(int responseTimeoutMs = Timeout.Infinite) : base(PipeMessageFactory.Instance) 
      => ResponseTimeoutMs = responseTimeoutMs;

    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    public int ResponseTimeoutMs { get; set; }

    /// <summary>
    /// Writes to the pipe directly and calls the Callback On Write
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>do not write to the pipe directly, use that instead, (or the Wrapping Client/Server)</remarks>
    public async Task WriteAsync(PipeMessage message, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);
      await Connection.WriteAsync(message, cts.Token).ConfigureAwait(false);
    }


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public override void OnConnected(string connection) { }


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public override void OnDisconnected(string connection) { }


    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public override void OnExceptionOccurred(Exception e) { }

    /// <summary>
    /// No-op handling of sent messages
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessageSent(PipeMessage message) { }

    /// <summary>
    /// Handles creation of Proxies
    /// Calls <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/> with broad generic type
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public override void OnMessageReceived(PipeMessage message)
    {
      if (message.Parameter is not string name || name == string.Empty)
      {
        throw new ArgumentException("Name was not specified.", nameof(message));
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
          await WriteAsync(PacketFactory.CreateCommand(command, pipeName), cancellationToken).ConfigureAwait(false);
        try
        {
          await proxy.StartAndConnectWithTimeoutAsync(ResponseTimeoutMs, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          if(primaryRequest)
            throw new NoResponseException(command, e);
          else 
            throw;
        }
        return proxy;
      }
      catch
      {
        await proxy.StopAsync(default).ConfigureAwait(false);
        throw;
      }
    }

    /// <summary>
    /// Create instances of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// It will be later cast to final type or the base interface by <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/>
    /// It will be later initialized by <see cref="ProvideProxyAsyncCore{T}(string, string, CancellationToken)"/>
    /// Custom logic needs to be provided to dispose elsewhere.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="pipeName">name for the pipe</param>
    /// <returns></returns>
    protected abstract IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName);
  }
}
