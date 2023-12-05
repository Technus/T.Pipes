using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
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
    Disposing = Busy | Sync,
    /// <summary>
    /// Finished Async Disposing no need for finalization
    /// </summary>
    DisposedAsync = Old | Async,
    /// <summary>
    /// Async Disposing
    /// </summary>
    DisposingAsync = Busy | Async,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    Finalized = Old | Destructor,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    Finalizing = Busy | Destructor,
  }

  /// <summary>
  /// Base patterns.
  /// </summary>
  public abstract class BaseClass : IAsyncDisposable, IDisposable
  {
    private int _disposedState = (int)DisposeState.New;

    /// <summary>
    /// Current dispose state
    /// </summary>
    protected DisposeState DisposeState => (DisposeState)_disposedState;

    /// <summary>
    /// Destructor helper
    /// </summary>
    protected void Finalizer()
    {
      var was = Interlocked.Exchange(ref _disposedState, (int)DisposeState.Finalizing);
      if (was == (int)DisposeState.New)
      {
        DisposeCore(disposing: false, includeAsync: true);
        _disposedState = (int)DisposeState.Finalized;
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
      var was = Interlocked.Exchange(ref _disposedState, (int)DisposeState.Disposing);
      if(was == (int)DisposeState.New)
      {
        DisposeCore(disposing: true, includeAsync: true);
        GC.SuppressFinalize(this);
        _disposedState = (int)DisposeState.Disposed;
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
      var was = Interlocked.Exchange(ref _disposedState, (int)DisposeState.DisposingAsync);
      if (was == (int)DisposeState.New)
      {
        await DisposeAsyncCore(disposing: true).ConfigureAwait(false);
        DisposeCore(disposing: true, includeAsync: false);
        GC.SuppressFinalize(this);
        _disposedState = (int)DisposeState.DisposedAsync;
      }
      else 
        throw new ObjectDisposedException(GetType().Name, $"Previously was: {(DisposeState)was}");
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
