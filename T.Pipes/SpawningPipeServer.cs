using System.Diagnostics;
using System.Threading;
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
    /// <param name="client"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(string pipe, ProcessStartInfo client, TCallback callback) : base(new(pipe), client, callback)
    {
    }

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="client"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, ProcessStartInfo client, TCallback callback) : base(pipe, client, callback)
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
    private readonly Process _process;

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="client"></param>
    /// <param name="callback"></param>
    public SpawningPipeServer(TPipe pipe, ProcessStartInfo client, TCallback callback) : base(pipe, callback)
      => _process = new Process { StartInfo = client };

    /// <summary>
    /// Disposes <see cref="PipeConnection{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnection{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if (includeAsync)
        Callback.Dispose();
      _process.Close();
      _process.Dispose();
    }

    /// <summary>
    /// Disposes <see cref="PipeConnection{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnection{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await Callback.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// starts the process and the pipe
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      _process.Start();
      return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// stops the process and the pipe
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
      _process.Close();
      return base.StopAsync(cancellationToken);
    }
  }
}
