using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServer<TCallback> 
    : SpawningPipeServer<H.Pipes.PipeServer<PipeMessage>, TCallback>
    where TCallback : SpawningPipeCallback
  {
    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }


  /// <summary>
  /// Helper to wrap factorization of <see cref="T.Pipes.Abstractions.IPipeDelegatingConnection{TMessage}"/><br/>
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class SpawningPipeServer<TPipe, TCallback> 
    : PipeServer<TPipe, PipeMessage, TCallback>
    where TCallback : SpawningPipeCallback
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
  {

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(TPipe pipe, TCallback callback) : base(pipe, callback) 
    { 
    }
  }
}
