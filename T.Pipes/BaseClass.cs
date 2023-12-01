using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <summary>
  /// Possible object states
  /// </summary>
  public enum DisposeState : int
  {
    /// <summary>
    /// Created
    /// </summary>
    New,
    /// <summary>
    /// Disposing or Async Disposing
    /// </summary>
    Disposing,
    /// <summary>
    /// Finished Disposing no need for finalization
    /// </summary>
    Disposed,
    /// <summary>
    /// Finalizer was called
    /// </summary>
    Finalizing,
    /// <summary>
    /// Was Finalized instead of disposing
    /// </summary>
    Finalized,
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
        DisposeCore(disposing: false,includeAsync: true);
        _disposedState = (int)DisposeState.Finalized;
      }
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
    }

    /// <summary>
    /// Handle dispose logic async
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    public async ValueTask DisposeAsync()
    {
      var was = Interlocked.Exchange(ref _disposedState, (int)DisposeState.Disposing);
      if (was == (int)DisposeState.New)
      {
        await DisposeAsyncCore(disposing: true).ConfigureAwait(false);
        DisposeCore(disposing: true, includeAsync: false);
        GC.SuppressFinalize(this);
        _disposedState = (int)DisposeState.Disposed;
      }
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
    protected virtual ValueTask DisposeAsyncCore(bool disposing) => default;
  }
}
