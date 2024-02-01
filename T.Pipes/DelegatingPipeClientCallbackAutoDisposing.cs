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
    where TTarget : IDisposable
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
      if(includeAsync || Target is not IAsyncDisposable)
        Target.Dispose();
    }

    /// <summary>
    /// Also disposes Target
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      if (Target is IAsyncDisposable disposable)
        await disposable.DisposeAsync().ConfigureAwait(false);
    }
  }
}
