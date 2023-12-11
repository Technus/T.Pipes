﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Possible object states
  /// </summary>
  [Flags]
  public enum DisposeState : int
  {
    /// <summary>
    /// Created
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Created
    /// </summary>
    New = 0x00,
    /// <summary>
    /// Disposed or disposing
    /// </summary>
    Old = 0x10,
    /// <summary>
    /// In process of disposing
    /// </summary>
    Busy = 0x20,
    /// <summary>
    /// Sync
    /// </summary>
    Sync = 0x01,
    /// <summary>
    /// Async
    /// </summary>
    Async = 0x02,
    /// <summary>
    /// In process of disposing
    /// </summary>
    Destructor = 0x04,

    /// <summary>
    /// Finished Disposing no need for finalization
    /// </summary>
    Disposed = Old | Sync,
    /// <summary>
    /// Disposing
    /// </summary>
    Disposing = Busy | Disposed,
    /// <summary>
    /// Finished Async Disposing no need for finalization
    /// </summary>
    DisposedAsync = Old | Async,
    /// <summary>
    /// Async Disposing
    /// </summary>
    DisposingAsync = Busy | DisposedAsync,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    Finalized = Old | Destructor,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    Finalizing = Busy | Finalized,
  }
}
