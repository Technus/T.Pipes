using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServer<TCallback> 
    : SpawningPipeServer<H.Pipes.PipeServer<PipeMessage>, TCallback>
    where TCallback : SpawningPipeServerCallback
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
    where TCallback : SpawningPipeCallback<TPipe>
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


    /// <summary>
    /// Disposes <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if (includeAsync)
        Callback.Dispose();
    }

    /// <summary>
    /// Disposes <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await Callback.DisposeAsync().ConfigureAwait(false);
    }
  }
}
