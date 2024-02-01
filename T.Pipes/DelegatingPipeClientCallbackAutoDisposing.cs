using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <summary>
  /// Helper to handle Surrogate client side Pipe disposing, and Target disposing
  /// </summary>
  /// <typeparam name="TTarget"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class DelegatingPipeClientCallbackAutoDisposing<TTarget, TCallback>
  : DelegatingPipeClientCallbackSelfDisposing<TTarget, TCallback>
    where TCallback : DelegatingPipeClientCallbackAutoDisposing<TTarget, TCallback>
  {
    /// <summary>
    /// Creates callback
    /// </summary>
    /// <param name="target"></param>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeClientCallbackAutoDisposing(TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(target, responseTimeoutMs)
    {
    }

    /// <summary>
    /// Also disposes Target
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);

      if(Target is IDisposable disposable)
      {
        if (includeAsync || Target is not IAsyncDisposable)
        {
          disposable.Dispose();
        }
      }
      else if (includeAsync && Target is IAsyncDisposable asyncDisposable)
      {
        var disposeTask = asyncDisposable.DisposeAsync();
        if (!disposeTask.IsCompleted)
          disposeTask.AsTask().Wait();
      }
    }

    /// <summary>
    /// Also disposes Target
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      if (Target is IAsyncDisposable asyncDisposable)
        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
    }
  }
}
