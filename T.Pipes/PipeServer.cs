using H.Formatters;
using H.Pipes.Args;
using System;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public class PipeServer<TCallback>
    : PipeServer<H.Pipes.PipeServer<PipeMessage>, PipeMessage, TCallback>
    where TCallback : IPipeCallback<PipeMessage>
  {
    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public class PipeServer<TPacket, TCallback>
    : PipeServer<H.Pipes.PipeServer<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(H.Pipes.PipeServer<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <summary>
  /// Base pipe server implementation
  /// </summary>
  /// <typeparam name="TPipe"><see cref="H.Pipes.IPipeServer{TPacket}"/></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TCallback"><see cref="IPipeCallback{TPacket}"/></typeparam>
  public class PipeServer<TPipe, TPacket, TCallback>
    : PipeConnectionBase<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Same as <see cref="ServerName"/>
    /// </summary>
    public override string PipeName => Pipe.PipeName;

    /// <inheritdoc/>
    public override string ServerName => Pipe.PipeName;

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.ClientDisconnected += OnClientDisconnected;
      Pipe.ClientConnected += OnClientConnected;
    }

    private void OnClientDisconnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      DecrementConnectionCount();
      Callback.OnDisconnected(e.Connection.PipeName);
    }

    private void OnClientConnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      IncrementConnectionCount();
      Callback.OnConnected(e.Connection.PipeName);
    }

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      try
      {
        await NoOperations.WaitAsync(cts.Token).ConfigureAwait(false);
        try
        {
          await Pipe.StartAsync(cts.Token).ConfigureAwait(false);
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
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      await NoOperations.WaitAsync(cts.Token).ConfigureAwait(false);
      try
      {
        await Pipe.StopAsync(cts.Token).ConfigureAwait(false);
      }
      finally
      {
        NoOperations.Release();
      }
    }

    /// <summary>
    /// Starts and then awaits incoming connection, stops on failure/cancellation
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAndConnectAsync(CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);

#if NET5_0_OR_GREATER
      var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => tcs.TrySetResult();
#else
      var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => tcs.TrySetResult(null);
#endif

      CancellationTokenRegistration ctr = default;

      try
      {
#if NET5_0_OR_GREATER
        ctr = cts.Token.UnsafeRegister(static (x,ct) => ((TaskCompletionSource)x!).TrySetCanceled(ct), tcs);
#else
        ctr = cts.Token.Register(static x =>
        {
          var (tcs, ct) = ((TaskCompletionSource<object?>, CancellationToken))x;
          tcs.TrySetCanceled(ct);
        }, (tcs, cts.Token));
#endif

        Pipe.ClientConnected += onConnected;
        try
        {
          await NoOperations.WaitAsync(cts.Token).ConfigureAwait(false);
          try
          {
            await Pipe.StartAsync(cts.Token).ConfigureAwait(false);
          }
          finally
          {
            NoOperations.Release();
          }
          await tcs.Task.ConfigureAwait(false);
        }
        catch (Exception tcsException)
        {
          try
          {
            await StopAsync(default).ConfigureAwait(false);
          }
          catch (Exception stopException)
          {
            throw new AggregateException(tcsException,stopException);
          }
          throw;
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
        Pipe.ClientConnected -= onConnected;
      }
    }

    /// <inheritdoc/>
    protected override async Task StartAndConnectWithTimeoutInternalAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LifetimeCancellation.ThrowIfCancellationRequested();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(timeoutMs);
#if NET5_0_OR_GREATER
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => cts.CancelAfter(Timeout.Infinite);
#else
      EventHandler<ConnectionEventArgs<TPacket>> onConnected = (o, e) => cts.CancelAfter(Timeout.Infinite);
#endif
      try
      {
        Pipe.ClientConnected += onConnected;
        await StartAndConnectAsync(cts.Token).ConfigureAwait(false);
      }
      finally
      {
        Pipe.ClientConnected -= onConnected;
      }
    }

    /// <inheritdoc/>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      Pipe.ClientConnected -= OnClientConnected;
      Pipe.ClientDisconnected -= OnClientDisconnected;
    }
  }
}
