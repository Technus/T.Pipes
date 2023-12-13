using H.Formatters;
using H.Pipes.Args;
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
  public abstract class PipeConnectionBase<TPipe, TPacket, TCallback>
    : BaseClass, IPipeConnection<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    private int _connectionCount;
    private readonly SemaphoreSlim _noConnections = new(1, 1);

    /// <summary>
    /// The <typeparamref name="TPipe"/> used
    /// </summary>
    public TPipe Pipe { get; }

    /// <summary>
    /// The <typeparamref name="TCallback"/> used
    /// </summary>
    public TCallback Callback { get; }

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
    protected PipeConnectionBase(TPipe pipe, TCallback callback)
    {
      Pipe = pipe;
      Callback = callback;
      Pipe.ExceptionOccurred += OnExceptionOccurred;
      Pipe.MessageReceived += OnMessageReceived;
    }

    /// <summary>
    /// Must be called on incoming connection
    /// </summary>
    /// <returns>current count</returns>
    protected int IncrementConnectionCount()
    {
      var result = Interlocked.Increment(ref _connectionCount);
      if (result == 1) _noConnections.Wait();
      return result;
    }

    /// <summary>
    /// Must be called on closing connection
    /// </summary>
    /// <returns>current count</returns>
    protected int DecrementConnectionCount()
    {
      var result = Interlocked.Decrement(ref _connectionCount);
      if (result == 0) _noConnections.Release();
      return result;
    }

    private void OnMessageReceived(object? sender, ConnectionMessageEventArgs<TPacket?> e)
      => Callback.OnMessageReceived(e.Message!);

    private void OnExceptionOccurred(object? sender, ExceptionEventArgs e)
      => Callback.OnExceptionOccurred(e.Exception);

    /// <inheritdoc/>
    public virtual async Task WriteAsync(TPacket value, CancellationToken cancellationToken = default)
    {
      if(LifetimeCancellation.IsCancellationRequested)
        await Task.FromCanceled(LifetimeCancellation);
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      await Pipe.WriteAsync(value, cts.Token).ConfigureAwait(false);
      Callback.OnMessageSent(value);
    }

    /// <inheritdoc/>
    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task StartAndConnectAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public Task StartAndConnectWithTimeoutAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default)
    {
      if(timeoutMs >= 0)
        return StartAndConnectWithTimeoutInternalAsync(timeoutMs, cancellationToken);
      return StartAndConnectAsync(cancellationToken);
    }

    /// <summary>
    /// Actual start with timeout implementation
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    protected abstract Task StartAndConnectWithTimeoutInternalAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.StartAndConnectWithTimeoutAsync(int, CancellationToken)"/>
    /// And awaits, it will get cancelled on the <see cref="IPipeCallback{TMessage}.LifetimeCancellation"/> so on Dispose call
    /// </summary>
    /// <returns>Only after the client is Cancelled</returns>
    public async Task StartAndConnectWithTimeoutAndAwaitCancellationAsync(int timeoutMs = 1000, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      try
      {
        await StartAndConnectWithTimeoutAsync(timeoutMs, cts.Token).ConfigureAwait(false);
        //Let it spin...
        await Task.Delay(Timeout.Infinite, cts.Token).ConfigureAwait(false);
      }
      finally
      {
        await StopAsync(default).ConfigureAwait(false);
      }
    }

    /// <inheritdoc/>
    public async Task StartAsService(CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      while (!cts.Token.IsCancellationRequested)
      {
        await _noConnections.WaitAsync(cts.Token).ConfigureAwait(false);
        _noConnections.Release();
        cts.Token.ThrowIfCancellationRequested();
        try
        {
          await StartAndConnectAsync(cts.Token).ConfigureAwait(false);
        }
        finally
        {
          await Task.Delay(20).ConfigureAwait(false);
        }
      }
      cts.Token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Disposes <see cref="Pipe"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await Pipe.DisposeAsync().ConfigureAwait(false);
      await _noConnections.WaitAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes <see cref="Pipe"/> and <see cref="Callback"/>
    /// </summary>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      if (includeAsync)
      {
        var disposeTask = Pipe.DisposeAsync();
        if(!disposeTask.IsCompleted) 
          disposeTask.AsTask().Wait();
        _noConnections.Wait();
      }
      _noConnections.Dispose();
      Pipe.MessageReceived -= OnMessageReceived;
      Pipe.ExceptionOccurred -= OnExceptionOccurred;
    }

    /// <summary>
    /// Finalizer as this has unmanaged resources
    /// </summary>
    ~PipeConnectionBase() => Finalizer();
  }
}
