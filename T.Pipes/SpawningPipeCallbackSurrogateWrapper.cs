using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <summary>
  /// Helper to handle Surrogate Process Spawning/Stopping/Killing
  /// </summary>
  public abstract class SpawningPipeCallbackSurrogateWrapper : SpawningPipeCallback
  {
    private readonly SurrogateProcessWrapper _process;

    /// <summary>
    /// Create callback
    /// </summary>
    /// <param name="process"></param>
    /// <param name="responseTimeoutMs"></param>
    protected SpawningPipeCallbackSurrogateWrapper(SurrogateProcessWrapper process, int responseTimeoutMs = Timeout.Infinite) : base(responseTimeoutMs)
      => _process = process;

    /// <summary>
    /// Starts the Surrogate process
    /// </summary>
    public override void OnStarting()
    {
      base.OnStarting();
      _process.StartProcess().Wait();
    }

    /// <summary>
    /// Stops the Surrogate process
    /// </summary>
    public override void OnStopping()
    {
      base.OnStopping();
      _process.StopProcess().Wait();
    }

    /// <summary>
    /// Also disposes the Surrogate by stopping or killing it
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if(includeAsync)
        _process.Dispose();
    }

    /// <summary>
    /// Also disposes the Surrogate by stopping or killing it
    /// </summary>
    /// <param name="disposing"></param>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await _process.DisposeAsync().ConfigureAwait(false);
    }
  }
}
