using System;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public  class DelegatingPipeServer<TTargetAndCallback>
    : DelegatingPipeServer<TTargetAndCallback, TTargetAndCallback>
    where TTargetAndCallback : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, PipeMessage, PipeMessageFactory, TTargetAndCallback, TTargetAndCallback>, IDisposable
  {
    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(string pipe, TTargetAndCallback callback) : base(pipe, callback)
    {
    }

    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TTargetAndCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeServer<TTarget, TCallback>
    : DelegatingPipeServer<H.Pipes.PipeServer<PipeMessage>, TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeServer<TPipe, TTarget, TCallback>
    : DelegatingPipeServer<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeServer<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeServer<TPipe, TPacket, TCallback>, IPipeDelegatingConnection<TPacket>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TPacket : IPipeMessage
    where TTarget : IDisposable
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
  {
    /// <summary>
    /// <see cref="DelegatingPipeCallback{TPipe, TPacket, TPacketFactory, TTarget, TCallback}.Target"/>
    /// </summary>
    public TTarget Target => Callback.Target;

    IPipeDelegatingCallback<TPacket> IPipeDelegatingConnection<TPacket>.Callback => Callback;

    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(TPipe pipe, TCallback callback)
      : base(pipe, callback)
    {
    }

    /// <summary>
    /// Disposes <see cref="PipeConnection{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnection{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <returns></returns>
    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }
  }
}
