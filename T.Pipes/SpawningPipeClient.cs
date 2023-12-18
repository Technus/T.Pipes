using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeClient<TCallback> 
    : SpawningPipeClient<H.Pipes.PipeClient<PipeMessage>, TCallback>
    where TCallback : SpawningPipeClientCallback
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
    where TCallback : SpawningPipeCallback<TPipe>
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
