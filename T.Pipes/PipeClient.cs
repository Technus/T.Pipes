using H.Formatters;
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
    public override string PipeName => Pipe.PipeName;

    /// <inheritdoc/>
    public override string ServerName => Pipe.ServerName;

    /// <summary>
    /// Creates the base pipe client implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.AutoReconnect = true;
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
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      if(cancellationToken.IsCancellationRequested)
        await Task.FromCanceled(cancellationToken).ConfigureAwait(false);
      if(LifetimeCancellation.IsCancellationRequested)
        await Task.FromCanceled(LifetimeCancellation).ConfigureAwait(false);

      _ = StartAndConnectAsync(cancellationToken);
      await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken = default)
      => Pipe.DisconnectAsync(cancellationToken);

    /// <inheritdoc/>
    public override async Task StartAndConnectAsync(CancellationToken cancellationToken = default)
    {
      if (cancellationToken.IsCancellationRequested)
        await Task.FromCanceled(cancellationToken).ConfigureAwait(false);
      if (LifetimeCancellation.IsCancellationRequested)
        await Task.FromCanceled(LifetimeCancellation).ConfigureAwait(false);

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      try
      {
        await Pipe.ConnectAsync(cts.Token).ConfigureAwait(false);
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
    protected override async Task StartAndConnectWithTimeoutInternalAsync(int timeoutMs, CancellationToken cancellationToken = default)
    {
      if (cancellationToken.IsCancellationRequested)
        await Task.FromCanceled(cancellationToken).ConfigureAwait(false);
      if (LifetimeCancellation.IsCancellationRequested)
        await Task.FromCanceled(LifetimeCancellation).ConfigureAwait(false);

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      cts.CancelAfter(timeoutMs);
#if NET5_0_OR_GREATER
      var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => { cts.CancelAfter(Timeout.Infinite); tcs.TrySetResult(); };
#else
      var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => { cts.CancelAfter(Timeout.Infinite); tcs.TrySetResult(null); };
#endif
      CancellationTokenRegistration ctr = default;
      try
      {
#if NET5_0_OR_GREATER
        ctr = cts.Token.UnsafeRegister(static (x, ct) => ((TaskCompletionSource)x!).TrySetCanceled(ct), tcs);
#else
        ctr = cts.Token.Register(static x =>
        {
          var (tcs, ct) = ((TaskCompletionSource<object?>, CancellationToken))x;
          tcs.TrySetCanceled(ct);
        }, (tcs, cts.Token));
#endif

        Pipe.Connected += onConnected;
        await Task.WhenAll(StartAndConnectAsync(cts.Token), tcs.Task).ConfigureAwait(false);
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
      {
        tcs.TrySetException(ex);
        if (cancellationToken.IsCancellationRequested)
        {
          throw;
        }
        else
        {
          throw new TimeoutException("Timeout expired", ex);
        }
      }
      catch (Exception e)
      {
        tcs.TrySetException(e);
        throw;
      }
      finally
      {
        ctr.Dispose();
        if (!tcs.Task.IsCompleted)
          tcs.TrySetException(new InvalidOperationException("Failed to finish gracefully."));
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
