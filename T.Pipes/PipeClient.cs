using H.Pipes.Args;
using System;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public class PipeClient<TCallback>
    : PipeClient<H.Pipes.PipeClient<PipeMessage>, PipeMessage, TCallback>
    where TCallback : IPipeCallback<PipeMessage>
  {
    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public class PipeClient<TPacket, TCallback>
    : PipeClient<H.Pipes.PipeClient<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(H.Pipes.PipeClient<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <summary>
  /// Base pipe client implementation
  /// </summary>
  /// <typeparam name="TPipe"><see cref="H.Pipes.IPipeServer{TPacket}"/></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TCallback"><see cref="IPipeCallback{TPacket}"/></typeparam>
  public class PipeClient<TPipe, TPacket, TCallback>
    : PipeConnectionBase<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Generated unique name
    /// </summary>
    public sealed override string PipeName => Pipe.PipeName;

    /// <inheritdoc/>
    public sealed override string ServerName => Pipe.ServerName;

    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.AutoReconnect = false;
      Pipe.Disconnected += OnDisconnected;
      Pipe.Connected += OnConnected;
    }

    private void OnDisconnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      DecrementConnectionCount();
      Callback.OnDisconnected(e.Connection.PipeName);
    }

    private void OnConnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      IncrementConnectionCount();
      Callback.OnConnected(e.Connection.PipeName);
    }

    /// <inheritdoc/>
    public sealed override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      _ = StartAndConnectAsync(cancellationToken);
      await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public sealed override async Task StopAsync(CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      await NoOperations.WaitAsync(cts.Token).ConfigureAwait(false);
      try
      {
        Callback.OnStopping();
        await Pipe.DisconnectAsync(cts.Token).ConfigureAwait(false);
        Callback.OnStopped();
      }
      finally
      {
        NoOperations.Release();
      }
    }

    /// <inheritdoc/>
    public sealed override async Task StartAndConnectAsync(CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      try
      {
        await NoOperations.WaitAsync(cts.Token).ConfigureAwait(false);
        try
        {
          Callback.OnStarting();
          await Pipe.ConnectAsync(cts.Token).ConfigureAwait(false);
          Callback.OnStarted();
        }
        finally
        {
          NoOperations.Release();
        }
      }
      catch (Exception startException)
      {
        try
        {
          await StopAsync(default).ConfigureAwait(false);
        }
        catch (Exception stopException)
        {
          throw new AggregateException(startException, stopException);
        }
        throw;
      }
    }

    /// <inheritdoc/>
    protected sealed override async Task StartAndConnectWithTimeoutInternalAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      cts.CancelAfter(timeoutMs);
#if NET5_0_OR_GREATER
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => cts.CancelAfter(Timeout.Infinite);
#else
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => cts.CancelAfter(Timeout.Infinite);
#endif
      try
      {
        Pipe.Connected += onConnected;
        await StartAndConnectAsync(cts.Token).ConfigureAwait(false);
      }
      finally
      {
        Pipe.Connected -= onConnected;
      }
    }

    /// <inheritdoc/>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      Pipe.Connected -= OnConnected;
      Pipe.Disconnected -= OnDisconnected;
    }
  }
}
