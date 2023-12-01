﻿using H.Pipes.Args;
using System;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Base class used for creating pipes
  /// </summary>
  /// <typeparam name="TPipe">any <see cref="H.Pipes.IPipeConnection{TPacket}"/></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TCallback">any <see cref="IPipeCallback{TPacket}"/></typeparam>
  public abstract class PipeConnection<TPipe, TPacket, TCallback>
    : BaseClass, IPipeConnection<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// The <typeparamref name="TPipe"/> used
    /// </summary>
    public TPipe Pipe { get; }

    /// <summary>
    /// The <typeparamref name="TCallback"/> used
    /// </summary>
    public TCallback Callback { get; }

    /// <inheritdoc/>
    public abstract bool IsRunning { get; }

    /// <inheritdoc/>
    public abstract string PipeName { get; }

    /// <inheritdoc/>
    public abstract string ServerName { get; }

    IPipeCallback<TPacket> IPipeConnection<TPacket>.Callback => Callback;

    /// <summary>
    /// Creates the base pipe implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected PipeConnection(TPipe pipe, TCallback callback)
    {
      Pipe = pipe;
      Callback = callback;

      Pipe.ExceptionOccurred += OnExceptionOccurred;
      Pipe.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object? sender, ConnectionMessageEventArgs<TPacket?> e) 
      => Callback.OnMessageReceived(e.Message);

    private void OnExceptionOccurred(object? sender, ExceptionEventArgs e) 
      => Callback.OnExceptionOccurred(e.Exception);

    /// <inheritdoc/>
    public async Task WriteAsync(TPacket value, CancellationToken cancellationToken = default)
    {
      await Pipe.WriteAsync(value, cancellationToken).ConfigureAwait(false);
      Callback.OnMessageSent(value);
    }

    /// <inheritdoc/>
    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task StartAndConnectAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public Task StartAndConnectWithTimeoutAsync(int timeoutMs = Timeout.Infinite, CancellationToken cancellationToken = default)
    {
      if(timeoutMs >= 0)
      {
        return StartAndConnectWithTimeoutInternalAsync(timeoutMs, cancellationToken);
      }
      return StartAndConnectAsync(cancellationToken);
    }

    /// <summary>
    /// Actual start with timeout implementation
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    protected async Task StartAndConnectWithTimeoutInternalAsync(int timeoutMs, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(timeoutMs);
      try
      {
        await StartAndConnectAsync(cts.Token).ConfigureAwait(false);
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          throw;
        }
        else
        {
          throw new TimeoutException("Timeout expired", ex);
        }
      }
    }

    /// <summary>
    /// Disposes <see cref="Pipe"/>
    /// </summary>
    /// <returns></returns>
    protected override ValueTask DisposeAsyncCore(bool disposing)
      => Pipe.DisposeAsync();

    /// <summary>
    /// Disposes <see cref="Pipe"/> and <see cref="Callback"/>
    /// </summary>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      if (includeAsync)
        Pipe.DisposeAsync().GetAwaiter().GetResult();
      Pipe.MessageReceived -= OnMessageReceived;
      Pipe.ExceptionOccurred -= OnExceptionOccurred;
    }

    /// <summary>
    /// Finalizer as this has unmanaged resources
    /// </summary>
    ~PipeConnection() => Finalizer();
  }
}
