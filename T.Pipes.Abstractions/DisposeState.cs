using System;

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
    Old = 0x10,
    /// <summary>
    /// In process of disposing
    /// </summary>
    Busy = 0x20,

    /// <summary>
    /// <see cref="IDisposable.Dispose"/>
    /// </summary>
    Sync = 0x01,
    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync"/>
    /// </summary>
    Async = 0x02,
    /// <summary>
    /// <see cref="object.Finalize"/> descendants
    /// </summary>
    Finalize = 0x04,

    /// <summary>
    /// <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    Cancel = 0x08,

    /// <summary>
    /// <see cref="IDisposable.Dispose"/> after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    SyncAfterCancel = 0x100,
    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync"/> after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    AsyncAfterCancel = 0x200,
    /// <summary>
    /// <see cref="object.Finalize"/> descendants after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    FinalizeAfterCancel = 0x400,

    /// <summary>
    /// Any Dispose/Async/Finalize was called after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// </summary>
    AnyDispose = SyncAfterCancel | AsyncAfterCancel | FinalizeAfterCancel | Sync | Async | Finalize,

    /// <summary>
    /// <see cref="BaseClass.LifetimeCancellation"/> was called after cancelling <see cref="BaseClass.LifetimeCancellation"/>
    /// Should be unused...
    /// </summary>
    CancelAfterCancel = 0x800,

    /// <summary>
    /// Finished <see cref="IDisposable.Dispose"/> no need for finalization
    /// </summary>
    Disposed = Old | Sync,
    /// <summary>
    /// <see cref="IDisposable.Dispose"/> was called no need for finalization
    /// </summary>
    Disposing = Busy | Disposed,
    /// <summary>
    /// Finished <see cref="IAsyncDisposable.DisposeAsync"/> no need for finalization
    /// </summary>
    DisposedAsync = Old | Async,
    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync"/> was called no need for finalization
    /// </summary>
    DisposingAsync = Busy | DisposedAsync,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    Finalized = Old | Finalize,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    Finalizing = Busy | Finalized,
    /// <summary>
    /// Cancelled <see cref="BaseClass.LifetimeCancellation"/> instead of disposing no need for finalization
    /// </summary>
    Cancelled = Old | Cancel,
    /// <summary>
    /// Finished cancelling <see cref="BaseClass.LifetimeCancellation"/> no need for finalization
    /// </summary>
    Cancelling = Busy | Cancelled,
  }
}
