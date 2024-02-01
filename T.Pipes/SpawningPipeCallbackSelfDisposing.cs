using System;
using System.Threading;

namespace T.Pipes
{
  /// <summary>
  /// Helper to handle Surrogate Process itself
  /// </summary>
  public abstract class SpawningPipeCallbackSelfDisposing : SpawningPipeCallback
  {
    /// <summary>
    /// Create callback
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected SpawningPipeCallbackSelfDisposing(int responseTimeoutMs = Timeout.Infinite) : base(responseTimeoutMs)
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
