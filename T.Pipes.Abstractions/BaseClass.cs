using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Base patterns.
  /// </summary>
  public abstract class BaseClass : IAsyncDisposable, IDisposable
  {
    /// <summary>
    /// <see cref="CancelCallback"/>
    /// </summary>
    private CancellationTokenRegistration _lifetimeCancellationRegistration;

    /// <summary>
    /// Current dispose state
    /// </summary>
    private int _disposeState = (int)DisposeState.New;

    /// <summary>
    /// Main Ctor initializing Dispose on cancellation
    /// </summary>
    protected BaseClass() => _lifetimeCancellationRegistration =
#if NET5_0_OR_GREATER
        LifetimeCancellation.UnsafeRegister(static x => ((BaseClass)x!).CancelCallback(), this);
#else
        LifetimeCancellation.Register(static x => ((BaseClass)x!).CancelCallback(), this);
#endif

    /// <summary>
    /// Signal that it is not needed anymore
    /// </summary>
    protected CancellationTokenSource LifetimeCancellationSource { get; } = new();

    /// <inheritdoc/>
    public CancellationToken LifetimeCancellation => LifetimeCancellationSource.Token;

    /// <summary>
    /// Check if disposed
    /// </summary>
    protected bool IsDisposed => _disposeState != (int)DisposeState.New;

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CancelCallback()
    {
      var was = Interlocked.Exchange(ref _disposeState, (int)DisposeState.Cancelling);
      if (was == (int)DisposeState.New)
      {
        Cancellation();
        DisposeCore(disposing: true, includeAsync: true);
        GC.SuppressFinalize(this);
        _disposeState = (int)DisposeState.Cancelled;
      }
      else
        throw new ObjectDisposedException(GetType().Name, $"Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Destructor helper
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Finalizer()
    {
      var was = Interlocked.Exchange(ref _disposeState, (int)DisposeState.Finalizing);
      if (was == (int)DisposeState.New)
      {
        Cancellation();
        DisposeCore(disposing: false, includeAsync: true);
        _disposeState = (int)DisposeState.Finalized;
      }
      else
        throw new ObjectDisposedException(GetType().Name, $"Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Handle dispose logic
    /// </summary>
    [DebuggerHidden]
    public void Dispose()
    {
      var was = Interlocked.Exchange(ref _disposeState, (int)DisposeState.Disposing);
      if (was == (int)DisposeState.New)
      {
        Cancellation();
        DisposeCore(disposing: true, includeAsync: true);
        GC.SuppressFinalize(this);
        _disposeState = (int)DisposeState.Disposed;
      }
      else
        throw new ObjectDisposedException(GetType().Name, $"Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Handle dispose logic async
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    public async ValueTask DisposeAsync()
    {
      var was = Interlocked.Exchange(ref _disposeState, (int)DisposeState.DisposingAsync);
      if (was == (int)DisposeState.New)
      {
        Cancellation();
        await DisposeAsyncCore(disposing: true).ConfigureAwait(false);
        DisposeCore(disposing: true, includeAsync: false);
        GC.SuppressFinalize(this);
        _disposeState = (int)DisposeState.DisposedAsync;
      }
      else
        throw new ObjectDisposedException(GetType().Name, $"Previously was: {(DisposeState)was}");
    }

    private void Cancellation()
    {
      _lifetimeCancellationRegistration.Dispose();
      LifetimeCancellationSource.Cancel();
      LifetimeCancellationSource.Dispose();
    }

    /// <summary>
    /// Extension point for handling sync dispose logic and sometimes also async
    /// </summary>
    /// <param name="disposing">false if called from finalizer</param>
    /// <param name="includeAsync">if should also dispose async disposables</param>
    protected virtual void DisposeCore(bool disposing, bool includeAsync) { }


    /// <summary>
    /// Extension point for handling async only dispose logic
    /// </summary>
    /// <param name="disposing">false if called from finalizer</param>
    /// <returns></returns>
    protected virtual ValueTask DisposeAsyncCore(bool disposing)
#if NET5_0_OR_GREATER
      => ValueTask.CompletedTask;
#else
      => default;
#endif
  }
}
