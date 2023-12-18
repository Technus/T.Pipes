using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Base Class for Callbacks
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TPacketFactory"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class PipeCallbackBase<TPipe, TPacket, TPacketFactory, TCallback>
    : BaseClass, IPipeCallback<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : PipeCallbackBase<TPipe, TPacket, TPacketFactory, TCallback>
  {
    /// <summary>
    /// Packet factory being used
    /// </summary>
    protected internal TPacketFactory PacketFactory { get; }

    /// <summary>
    /// Used to access data tunnel
    /// </summary>
    protected internal TPipe Pipe { get; }

    /// <summary>
    /// The base constructor
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="packetFactory"></param>
    protected PipeCallbackBase(TPipe pipe, TPacketFactory packetFactory)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
    }

    /// <inheritdoc/>
    public abstract void OnConnected(string connection);

    /// <inheritdoc/>
    public abstract void OnDisconnected(string connection);

    /// <inheritdoc/>
    public abstract void OnExceptionOccurred(Exception e);

    /// <inheritdoc/>
    public abstract void OnMessageReceived(TPacket message);

    /// <inheritdoc/>
    public abstract void OnMessageSent(TPacket message);
  }
}
