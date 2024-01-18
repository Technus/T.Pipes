using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeClient<TCallback> 
    : SpawningPipeClient<H.Pipes.PipeClient<PipeMessage>, TCallback>
    where TCallback : SpawningPipeCallback
  {
    /// <summary>
    /// Creates the Target implementation spawning code
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeClient(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the Target implementation spawning code
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }


  /// <summary>
  /// Helper to wrap Factorization of <see cref="T.Pipes.Abstractions.IPipeDelegatingConnection{TMessage}"/> Clients<br/>
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class SpawningPipeClient<TPipe, TCallback> 
    : PipeClient<TPipe, PipeMessage, TCallback>
    where TCallback : SpawningPipeCallback
    where TPipe : H.Pipes.IPipeClient<PipeMessage>
  {
    /// <summary>
    /// Creates the Target implementation spawning code
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }
}
