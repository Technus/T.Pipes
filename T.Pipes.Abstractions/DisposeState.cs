using System;
using System.Runtime.CompilerServices;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Possible object states
  /// </summary>
  [Flags]
#pragma warning disable S1939 // Inheritance list should not be redundant
  public enum DisposeState : int
#pragma warning restore S1939 // Inheritance list should not be redundant
  {
    /// <summary>
    /// Created
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Disposed or disposing
    /// </summary>
    Old = 1 << 28,
    /// <summary>
    /// In process of disposing
    /// </summary>
    Busy = 1 << 29,

    /// <summary>
    /// <see cref="BaseClass.Dispose"/>
    /// </summary>
    CheckedSync = 1 << 0,
    /// <summary>
    /// <see cref="BaseClass.DisposeAsync"/>
    /// </summary>
    CheckedAsync = 1 << 1,
    /// <summary>
    /// <see cref="BaseClass.FinalizerCore"/>
    /// </summary>
    CheckedFinalize = 1 << 2,
    /// <summary>
    /// <see cref="BaseClass.RegisterTryCancelOnCancellation"/>
    /// </summary>
    CheckedCancel = 1 << 3,

    /// <summary>
    /// Any NonTry Was Called
    /// </summary>
    AnyChecked = CheckedSync | CheckedAsync | CheckedFinalize | CheckedCancel,

    /// <summary>
    /// <see cref="BaseClass.TryDisposeCore"/>
    /// </summary>
    TrySync = 1 << 4,
    /// <summary>
    /// <see cref="BaseClass.TryDisposeAsync"/>
    /// </summary>
    TryAsync = 1 << 5,
    /// <summary>
    /// <see cref="BaseClass.TryFinalizerCore"/>
    /// </summary>
    TryFinalize = 1 << 6,
    /// <summary>
    /// <see cref="BaseClass.RegisterTryCancelOnCancellation"/>
    /// </summary>
    TryCancel = 1 << 7,

    /// <summary>
    /// Any Try Was called
    /// </summary>
    AnyTry = TrySync | TryAsync | TryFinalize | TryCancel,

    /// <summary>
    /// <see cref="BaseClass.Dispose"/> after any <see cref="AnySafe"/>
    /// </summary>
    SyncAfterTry = 1 << 8,
    /// <summary>
    /// <see cref="BaseClass.DisposeAsync"/> after any <see cref="AnySafe"/>
    /// </summary>
    AsyncAfterTry = 1 << 9,
    /// <summary>
    /// <see cref="BaseClass.FinalizerCore"/> descendants after any <see cref="AnySafe"/>
    /// </summary>
    FinalizeAfterTry = 1 << 10,
    /// <summary>
    /// <see cref="BaseClass.LifetimeCancellation"/> was called after any <see cref="AnySafe"/>
    /// </summary>
    CancelAfterTry = 1 << 11,

    /// <summary>
    /// Any AfterCancel Was called
    /// </summary>
    AnyAfterTry = SyncAfterTry | AsyncAfterTry | FinalizeAfterTry | CancelAfterTry,

    /// <summary>
    /// <see cref="BaseClass.Dispose"/> after any safe <see cref="AnySafe"/>
    /// </summary>
    SyncAfterCancel = 1 << 12,
    /// <summary>
    /// <see cref="BaseClass.DisposeAsync"/> after any safe <see cref="AnySafe"/>
    /// </summary>
    AsyncAfterCancel = 1 << 13,
    /// <summary>
    /// <see cref="BaseClass.FinalizerCore"/> descendants after any safe <see cref="AnySafe"/>
    /// </summary>
    FinalizeAfterCancel = 1 << 14,
    /// <summary>
    /// <see cref="BaseClass.LifetimeCancellation"/> was called after any safe <see cref="AnySafe"/>
    /// Should be unused...
    /// </summary>
    CancelAfterCancel = 1 << 15,

    /// <summary>
    /// Any AfterCancel Was called
    /// </summary>
    AnyAfterCancel = SyncAfterCancel | AsyncAfterCancel | FinalizeAfterCancel | CancelAfterCancel,

    /// <summary>
    /// Any Non Try Was called
    /// </summary>
    AnyNonTry = AnyChecked | AnyAfterCancel | AnyAfterTry,

    /// <summary>
    /// Any Try/Dispose/Async/Finalize was called after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    AnyDispose = CheckedSync | CheckedAsync | CheckedFinalize 
      | SyncAfterCancel | AsyncAfterCancel | FinalizeAfterCancel
      | SyncAfterTry | AsyncAfterTry | FinalizeAfterTry,

    /// <summary>
    /// Any Try/Dispose/Async/Finalize was called after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    AnyCancel = CheckedCancel | CancelAfterCancel | CancelAfterTry,

    /// <summary>
    /// Finished <see cref="IDisposable.Dispose"/> no need for finalization
    /// </summary>
    Disposed = Old | CheckedSync,
    /// <summary>
    /// <see cref="IDisposable.Dispose"/> was called no need for finalization
    /// </summary>
    Disposing = Busy | Disposed,
    /// <summary>
    /// Finished <see cref="IAsyncDisposable.DisposeAsync"/> no need for finalization
    /// </summary>
    DisposedAsync = Old | CheckedAsync,
    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync"/> was called no need for finalization
    /// </summary>
    DisposingAsync = Busy | DisposedAsync,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    Finalized = Old | CheckedFinalize,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    Finalizing = Busy | Finalized,
    /// <summary>
    /// Cancelled <see cref="BaseClass.LifetimeCancellation"/> instead of disposing no need for finalization
    /// </summary>
    Cancelled = Old | CheckedCancel,
    /// <summary>
    /// Finished cancelling <see cref="BaseClass.LifetimeCancellation"/> no need for finalization
    /// </summary>
    Cancelling = Busy | Cancelled,

    /// <summary>
    /// Finished <see cref="IDisposable.Dispose"/> no need for finalization
    /// </summary>
    TryDisposed = Old | TrySync,
    /// <summary>
    /// <see cref="IDisposable.Dispose"/> was called no need for finalization
    /// </summary>
    TryDisposing = Busy | TryDisposed,
    /// <summary>
    /// Finished <see cref="IAsyncDisposable.DisposeAsync"/> no need for finalization
    /// </summary>
    TryDisposedAsync = Old | TryAsync,
    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync"/> was called no need for finalization
    /// </summary>
    TryDisposingAsync = Busy | TryDisposedAsync,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    TryFinalized = Old | TryFinalize,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    TryFinalizing = Busy | TryFinalized,
    /// <summary>
    /// Cancelled <see cref="BaseClass.LifetimeCancellation"/> instead of disposing no need for finalization
    /// </summary>
    TryCancelled = Old | TryCancel,
    /// <summary>
    /// Finished cancelling <see cref="BaseClass.LifetimeCancellation"/> no need for finalization
    /// </summary>
    TryCancelling = Busy | TryCancelled,
  }

  internal static class DisposeStateExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool NoneIn(this DisposeState state, int value) => (value & (int)state) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool AnyIn(this DisposeState state, int value) => (value & (int)state) != 0;
  }
}
