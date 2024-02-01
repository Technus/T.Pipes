using System;
using System.Threading;

namespace T.Pipes
{
  /// <summary>
  /// Helper to handle Surrogate client side Pipe disposing
  /// </summary>
  /// <typeparam name="TTarget"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class DelegatingPipeClientCallbackSelfDisposing<TTarget, TCallback>
  : DelegatingPipeClientCallback<TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeClientCallbackSelfDisposing<TTarget, TCallback>
  {
    /// <summary>
    /// Creates callback
    /// </summary>
    /// <param name="target"></param>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeClientCallbackSelfDisposing(TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(target, responseTimeoutMs)
    {
    }

    /// <summary>
    /// Disposes itself on Disconnection, to allow detection of invalid state
    /// </summary>
    /// <param name="connection"></param>
    public override void OnDisconnected(string connection)
    {
      base.OnDisconnected(connection);
      Dispose();
    }

    /// <summary>
    /// Disposes itself on Exception, to allow detection of invalid state
    /// </summary>
    /// <param name="exception"></param>
    public override void OnExceptionOccurred(Exception exception)
    {
      base.OnExceptionOccurred(exception);
      Dispose();
    }
  }
}
