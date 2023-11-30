using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeClient<TCallback> :
    SpawningPipeClient<H.Pipes.PipeClient<PipeMessage>, TCallback>
    where TCallback : SpawningPipeClientCallback<H.Pipes.PipeClient<PipeMessage>>
  {
    /// <summary>
    /// Creates the Target implementation spawning code
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="callback"></param>
    protected SpawningPipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
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
  /// implement <see cref="SpawningPipeClientCallback{TPipe}.CreateProxy(PipeMessage)"/> to provide implementations
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class SpawningPipeClient<TPipe, TCallback> : PipeClient<TPipe, PipeMessage, TCallback>
    where TCallback : SpawningPipeClientCallback<TPipe>
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
    /// Disposes the Pipe and Callback
    /// </summary>
    /// <returns></returns>
    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync().ConfigureAwait(false);
      await Callback.DisposeAsync().ConfigureAwait(false);
    }
  }
}
