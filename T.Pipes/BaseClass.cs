using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <summary>
  /// Base patterns.
  /// </summary>
  public abstract class BaseClass : IAsyncDisposable, IDisposable
  {
    private enum DisposeState : int
    {
      New,
      Disposing,
      Disposed,
    }

    private int _disposedState = (int)DisposeState.New;

    /// <summary>
    /// Handle dispose logic
    /// </summary>
    [DebuggerHidden]
    public void Dispose()
    {
      var was = Interlocked.Exchange(ref _disposedState, (int)DisposeState.Disposing);
      if(was == (int)DisposeState.New)
      {
        DisposeCore(includeAsync: true);
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
        await DisposeAsyncCore().ConfigureAwait(false);
        DisposeCore(includeAsync: false);
        GC.SuppressFinalize(this);
        _disposedState = (int)DisposeState.Disposed;
      }
    }

    /// <summary>
    /// Extension point for handling sync dispose logic and sometimes also async
    /// </summary>
    /// <param name="includeAsync">if should also dispose async disposables</param>
    protected virtual void DisposeCore(bool includeAsync) { }


    /// <summary>
    /// Extension point for handling async only dispose logic
    /// </summary>
    /// <returns></returns>
    protected virtual ValueTask DisposeAsyncCore() => default;
  }
}
