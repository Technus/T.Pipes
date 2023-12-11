using System;
using System.Threading;
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
    /// Signal that it is not needed anymore
    /// </summary>
    protected CancellationTokenSource LifetimeCancellationSource { get; } = new();

    /// <inheritdoc/>
    public CancellationToken LifetimeCancellation 
      => LifetimeCancellationSource.Token;

    /// <summary>
    /// Packet factory being used
    /// </summary>
    protected TPacketFactory PacketFactory { get; }

    /// <summary>
    /// The base constructor
    /// </summary>
    /// <param name="packetFactory"></param>
    protected PipeCallbackBase(TPacketFactory packetFactory) 
      => PacketFactory = packetFactory;

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

    /// <inheritdoc/>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      LifetimeCancellationSource.Cancel();
      LifetimeCancellationSource.Dispose();
    }
  }
}
