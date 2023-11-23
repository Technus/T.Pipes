using System;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class DelegatingPipeClient<TTarget, TCallback>
      : DelegatingPipeClient<H.Pipes.PipeClient<PipeMessage>, TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget>
  {
    /// <summary>
    /// Creates the pipe client with a specified callback and pipe
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    protected DelegatingPipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
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
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
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
  /// Use in pair with <see cref="DelegatingPipeCallback{TPipe, TPacket, TPacketFactory, TTarget}"/>
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TPacketFactory"></typeparam>
  /// <typeparam name="TTarget"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class DelegatingPipeClient<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeClient<TPipe, TPacket, TCallback>, IPipeDelegatingConnection<TPacket>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget>
  {
    /// <summary>
    /// The actual <typeparamref name="TTarget"/> implementaion
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