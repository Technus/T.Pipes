using System;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class DelegatingPipeClient<TTarget, TCallback>
      : DelegatingPipeClient<H.Pipes.PipeClient<PipeMessage>, TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the pipe client with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeClient(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
    {
    }

    /// <summary>
    /// Creates the pipe client with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeClient<TPipe, TTarget, TCallback>
    : DelegatingPipeClient<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TPipe : H.Pipes.IPipeClient<PipeMessage>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the pipe client with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <summary>
  /// Pipe Client for hosting <typeparamref name="TTarget"/> implementation.
  /// </summary>
  /// <typeparam name="TPipe">data tunnel</typeparam>
  /// <typeparam name="TPacket">packet format</typeparam>
  /// <typeparam name="TPacketFactory">packet factory</typeparam>
  /// <typeparam name="TTarget">implementation type/interfaces</typeparam>
  /// <typeparam name="TCallback">response handler</typeparam>
  public abstract class DelegatingPipeClient<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeClient<TPipe, TPacket, TCallback>, IPipeDelegatingConnection<TPacket>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
  {
    /// <summary>
    /// The actual <typeparamref name="TTarget"/> implementation stored in <see cref="IPipeDelegatingConnection{TPacket}.Callback"/>
    /// </summary>
    public TTarget Target => Callback.Target;

    IPipeDelegatingCallback<TPacket> IPipeDelegatingConnection<TPacket>.Callback => Callback;

    /// <summary>
    /// Creates the pipe client with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeClient(TPipe pipe, TCallback callback)
      : base(pipe, callback)
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