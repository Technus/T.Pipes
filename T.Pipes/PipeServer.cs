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
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
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
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
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
    : PipeConnection<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Checks if it is running
    /// </summary>
    public override bool IsRunning => Pipe.IsStarted;

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
      => Callback.Disconnected(e.Connection.PipeName);

    private void OnClientConnected(object? sender, ConnectionEventArgs<TPacket> e) 
      => Callback.Connected(e.Connection.PipeName);

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        await Pipe.StartAsync(cancellationToken).ConfigureAwait(false);
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
    public override Task StopAsync(CancellationToken cancellationToken = default)
      => Pipe.StopAsync(cancellationToken);

    /// <summary>
    /// Starts and then awaits incoming connection, stops on failure/cancellation
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAndConnectAsync(CancellationToken cancellationToken = default)
    {
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
        if (cancellationToken.CanBeCanceled)
#if NET5_0_OR_GREATER
          ctr = cancellationToken.UnsafeRegister(static (x,ct) 
            => ((TaskCompletionSource)x!).TrySetCanceled(ct), tcs);
#else
          ctr = cancellationToken.Register(static x =>
          {
            var (tcs, ct) = ((TaskCompletionSource<object?>, CancellationToken))x;
            tcs.TrySetCanceled(ct);
          }, (tcs, cancellationToken));
#endif

        Pipe.ClientConnected += onConnected;
        await StartAsync(cancellationToken).ConfigureAwait(false);
        try
        {
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
    protected override void DisposeCore(bool includeAsync)
    {
      base.DisposeCore(includeAsync);
      Pipe.ClientDisconnected -= OnClientDisconnected;
      Pipe.ClientConnected -= OnClientConnected;
    }
  }
}
