﻿using System;
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
    private int _disposeState = (int)DisposeState.None;

    /// <summary>
    /// Initialize Dispose on cancellation
    /// </summary>
    protected void DisposeOnCancellation() => _lifetimeCancellationRegistration =
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
    protected bool IsDisposed => _disposeState != (int)DisposeState.None;

    /// <summary>
    /// Gets the current dispose state <see cref="IsDisposed"/> as a better alternative
    /// </summary>
    protected DisposeState DisposeState => (DisposeState)_disposeState;

    /// <summary>
    /// Can be use to dispose on cancellation of <see cref="LifetimeCancellation"/>
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CancelCallback()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.Cancelling, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: true, includeAsync: true);

        LifetimeCancellationSource.Dispose();

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

        _disposeState &= ~(int)DisposeState.Busy;
      }
      else //Only one cancellation allowed
        throw new ObjectDisposedException(GetType().Name, $"Cancelling, Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Destructor helper
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Finalizer()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.Finalizing, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: false, includeAsync: true);

        LifetimeCancellationSource.Dispose();

        _disposeState &= ~(int)DisposeState.Busy;
      }
      else if ((was & (int)DisposeState.AnyDispose) == 0) //Only once more
        _disposeState |= (int)DisposeState.FinalizeAfterCancel;
      else
        throw new ObjectDisposedException(GetType().Name, $"Finalizing, Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Handle dispose logic
    /// </summary>
    [DebuggerHidden]
    public void Dispose()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.Disposing, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: true, includeAsync: true);

        LifetimeCancellationSource.Dispose();

        GC.SuppressFinalize(this);

        _disposeState &= ~(int)DisposeState.Busy;
      }
      else if ((was & (int)DisposeState.AnyDispose) == 0) //Only once more
        _disposeState |= (int)DisposeState.SyncAfterCancel;
      else
        throw new ObjectDisposedException(GetType().Name, $"Disposing, Previously was: {(DisposeState)was}");
    }

    /// <summary>
    /// Handle dispose logic async
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    public async ValueTask DisposeAsync()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.DisposingAsync, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
#if NET8_0_OR_GREATER
        await LifetimeCancellationSource.CancelAsync().ConfigureAwait(false);
#else
        LifetimeCancellationSource.Cancel();
#endif

        await DisposeAsyncCore(disposing: true).ConfigureAwait(false);
        DisposeCore(disposing: true, includeAsync: false);

        LifetimeCancellationSource.Dispose();

        GC.SuppressFinalize(this);

        _disposeState &= ~(int)DisposeState.Busy;
      }
      else if ((was & (int)DisposeState.AnyDispose) == 0) //Only once more
        _disposeState |= (int)DisposeState.AsyncAfterCancel;
      else
        throw new ObjectDisposedException(GetType().Name, $"Async Disposing, Previously was: {(DisposeState)was}");
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
