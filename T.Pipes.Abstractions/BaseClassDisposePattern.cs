using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  /// <inheritdoc/>
  public abstract class CheckedBaseClass : BaseClass
  {
    /// <summary>
    /// Disposes checked for redundant calls
    /// </summary>
    public sealed override void Dispose() => CheckedDispose();

    /// <summary>
    /// Disposes checked for redundant calls
    /// </summary>
    public sealed override ValueTask DisposeAsync() => CheckedDisposeAsync();

    /// <summary>
    /// Disposes checked for redundant calls
    /// </summary>
    ~CheckedBaseClass() => CheckedFinalizer();
  }

  /// <inheritdoc/>
  public abstract class UncheckedBaseClass : BaseClass
  {
    /// <summary>
    /// Disposes unchecked for redundant calls
    /// </summary>
    public sealed override void Dispose() => TryDispose();

    /// <summary>
    /// Disposes unchecked for redundant calls
    /// </summary>
    public sealed override ValueTask DisposeAsync() => TryDisposeAsync();

    /// <summary>
    /// Disposes unchecked for redundant calls
    /// </summary>
    ~UncheckedBaseClass() => TryFinalizer();
  }

  /// <summary>
  /// Base patterns.
  /// </summary>
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
  public abstract partial class BaseClass : IAsyncDisposable, IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
  {
    /// <summary>
    /// <see cref="CancelCore"/>
    /// </summary>
    private CancellationTokenRegistration _lifetimeCancellationRegistration;

    /// <summary>
    /// Current dispose state
    /// </summary>
    private int _disposeState = (int)DisposeState.None;

    /// <summary>
    /// Initialize Dispose on cancellation
    /// </summary>
    protected void RegisterTryCancelOnCancellation() => _lifetimeCancellationRegistration =
#if NET5_0_OR_GREATER
        LifetimeCancellation.UnsafeRegister(static x => ((BaseClass)x!).TryCancel(), this);
#else
        LifetimeCancellation.Register(static x => ((BaseClass)x!).TryCancel(), this);
#endif

    /// <summary>
    /// Initialize Dispose on cancellation
    /// </summary>
    protected void RegisterCheckedCancelOnCancellation() => _lifetimeCancellationRegistration =
#if NET5_0_OR_GREATER
        LifetimeCancellation.UnsafeRegister(static x => ((BaseClass)x!).CheckedCancel(), this);
#else
        LifetimeCancellation.Register(static x => ((BaseClass)x!).CheckedCancel(), this);
#endif

    /// <summary>
    /// Signal that it is not needed anymore
    /// </summary>
    internal CancellationTokenSource LifetimeCancellationSource { get; } = new();

    /// <summary>
    /// Signal that it is not needed anymore
    /// </summary>
    /// <remarks><see cref="LifetimeCancellationSource"/>.Token</remarks>
    public CancellationToken LifetimeCancellation => IsDisposed ? new CancellationToken(canceled: true) : LifetimeCancellationSource.Token;

    /// <summary>
    /// Check if disposed
    /// </summary>
    protected bool IsDisposed => _disposeState != (int)DisposeState.None;

    /// <summary>
    /// Check if disposed
    /// </summary>
    protected bool IsDisposing => (_disposeState & (int)DisposeState.Busy) != 0;

    /// <summary>
    /// Gets the current dispose state <see cref="IsDisposed"/> as a better alternative
    /// </summary>
    protected DisposeState DisposeState => (DisposeState)_disposeState;

    /// <summary>
    /// Can be use to dispose on cancellation of <see cref="LifetimeCancellation"/>
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckedCancel()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.Cancelling, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: true, includeAsync: true);

        LifetimeCancellationSource.Dispose();

#pragma warning disable CA1816, S3971 // Dispose methods should call SuppressFinalize
        GC.SuppressFinalize(this);
#pragma warning restore CA1816, S3971 // Dispose methods should call SuppressFinalize

        _disposeState &= ~(int)DisposeState.Busy;
      }
      else if (DisposeState.AnyNonTry.NoneIn(was)) //Only once more if no 'non Try' dispose methods were called
        _disposeState |= (int)DisposeState.CancelAfterTry;
      else //Only one cancellation allowed
        throw new ObjectDisposedException(GetType().Name, $"Cancelling, Dispose State was: {(DisposeState)was}");
    }

    /// <summary>
    /// Can be use to dispose on cancellation of <see cref="LifetimeCancellation"/>
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryCancel()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.TryCancelling, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: true, includeAsync: true);

        LifetimeCancellationSource.Dispose();

#pragma warning disable CA1816, S3971 // Dispose methods should call SuppressFinalize
        GC.SuppressFinalize(this);
#pragma warning restore CA1816, S3971 // Dispose methods should call SuppressFinalize

        _disposeState &= ~(int)DisposeState.Busy;
      }
    }

    /// <summary>
    /// Destructor helper, only one class in chain needs to use it in destructor
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void CheckedFinalizer()
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
      else if (DisposeState.AnyNonTry.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.FinalizeAfterTry;
      else if (DisposeState.AnyDispose.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.FinalizeAfterCancel;
      else
        throw new ObjectDisposedException(GetType().Name, $"Finalizing, Dispose State was: {(DisposeState)was}");
    }

    /// <summary>
    /// Destructor helper, only one class in chain needs to use it in destructor
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void TryFinalizer()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.TryFinalizing, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: false, includeAsync: true);

        LifetimeCancellationSource.Dispose();

        _disposeState &= ~(int)DisposeState.Busy;
      }
    }

    /// <summary>
    /// Handle dispose logic
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckedDispose()
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
      else if (DisposeState.AnyNonTry.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.SyncAfterTry;
      else if (DisposeState.AnyDispose.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.SyncAfterCancel;
      else
#pragma warning disable S3877 // Exceptions should not be thrown from unexpected methods
        throw new ObjectDisposedException(GetType().Name, $"Disposing, Dispose State was: {(DisposeState)was}");
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
    }

    /// <summary>
    /// Handle dispose logic
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryDispose()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.TryDisposing, (int)DisposeState.None);
      if (was == (int)DisposeState.None)
      {
        _lifetimeCancellationRegistration.Dispose();
        LifetimeCancellationSource.Cancel();

        DisposeCore(disposing: true, includeAsync: true);

        LifetimeCancellationSource.Dispose();

        GC.SuppressFinalize(this);

        _disposeState &= ~(int)DisposeState.Busy;
      }
    }

    /// <summary>
    /// Handle dispose logic async
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask CheckedDisposeAsync()
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
      else if (DisposeState.AnyNonTry.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.AsyncAfterTry;
      else if (DisposeState.AnyDispose.NoneIn(was)) //Only once more
        _disposeState |= (int)DisposeState.AsyncAfterCancel;
      else
        throw new ObjectDisposedException(GetType().Name, $"Async Disposing, Dispose State was: {(DisposeState)was}");
    }

    /// <summary>
    /// Handle dispose logic async
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask TryDisposeAsync()
    {
      var was = Interlocked.CompareExchange(ref _disposeState, (int)DisposeState.TryDisposingAsync, (int)DisposeState.None);
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
    }

    /// <summary>
    /// Should call <see cref="TryDispose"/> or <see cref="CheckedDispose"/>
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Should call <see cref="TryDisposeAsync"/> or <see cref="CheckedDisposeAsync"/>
    /// </summary>
    public abstract ValueTask DisposeAsync();

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
