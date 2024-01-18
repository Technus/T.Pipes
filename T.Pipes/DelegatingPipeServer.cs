using System;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Creates the pipe server with a specified callback and pipe
  /// </summary>
  /// <typeparam name="TTargetAndCallback">both proxy target and callback, this will satisfy that requirement</typeparam>
  public abstract class DelegatingPipeServer<TTargetAndCallback>
    : DelegatingPipeServer<TTargetAndCallback, TTargetAndCallback>
    where TTargetAndCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTargetAndCallback, TTargetAndCallback>, IDisposable
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
    where TCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the pipe server with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeServer(string pipe, TCallback callback) : base(new(pipe, formatter: new Formatter()), callback)
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
    where TCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTarget, TCallback>
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

  /// <summary>
  /// Pipe Server for accessing remote <typeparamref name="TTarget"/> implementation.
  /// </summary>
  /// <typeparam name="TPipe">data tunnel</typeparam>
  /// <typeparam name="TPacket">packet format</typeparam>
  /// <typeparam name="TPacketFactory">packet factory</typeparam>
  /// <typeparam name="TTarget">implementation type/interfaces</typeparam>
  /// <typeparam name="TCallback">response handler</typeparam>
  public abstract class DelegatingPipeServer<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeServer<TPipe, TPacket, TCallback>, IPipeDelegatingConnection<TPacket>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TPacket : IPipeMessage
    where TTarget : IDisposable
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPacket, TPacketFactory, TTarget, TCallback>
  {
    /// <summary>
    /// <see cref="DelegatingPipeCallback{TPacket, TPacketFactory, TTarget, TCallback}.Target"/>
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
  }
}
