﻿using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Base Class for Callbacks
  /// </summary>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TPacketFactory"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class PipeCallbackBase<TPacket, TPacketFactory, TCallback>
    : BaseClass, IPipeCallback<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : PipeCallbackBase<TPacket, TPacketFactory, TCallback>
  {
    private IPipeConnection<TPacket>? _connection;

    /// <summary>
    /// Packet factory being used
    /// </summary>
    protected internal TPacketFactory PacketFactory { get; }

    /// <summary>
    /// Used to access data tunnel should be set immediately after calling ctor
    /// </summary>
    public virtual IPipeConnection<TPacket> Connection
    {
      get => _connection!;
      set
      {
        if (_connection is null)
          _connection = value ?? throw new InvalidOperationException("Trying to set to null.");
        else
          throw new InvalidOperationException("Value already set");
      }
    }

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
  }
}
