﻿using System;
using System.Threading;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Factory for <see cref="PipeMessage"/>
  /// </summary>
  public class PipeMessageFactory : IPipeMessageFactory<PipeMessage>
  {
    /// <summary>
    /// Public instance to use
    /// </summary>
    public static PipeMessageFactory Instance { get; } = new();

    private long _packetId;

    /// <summary>
    /// Ctor exposed for tests and derived classes
    /// </summary>
    protected internal PipeMessageFactory() 
      => _packetId = (long)new Random().Next() * 819200000;

    /// <inheritdoc/>
    public PipeMessage CreateCommand(string command) 
      => new() { Command = command, Id = Interlocked.Increment(ref _packetId), PacketType = PacketType.Command };

    /// <inheritdoc/>
    public PipeMessage CreateCommand(string command, object? parameter) 
      => new() { Command = command, Id = Interlocked.Increment(ref _packetId), PacketType = PacketType.Command, Parameter = parameter };

    /// <inheritdoc/>
    public PipeMessage CreateCommandCancellation(string command, Exception? parameter = default)
      => new() { Command = command, Id = Interlocked.Increment(ref _packetId), PacketType = PacketType.CommandCancellation, Parameter = parameter };

    /// <inheritdoc/>
    public PipeMessage CreateCommandFailure(string command, Exception parameter)
      => new() { Command = command, Id = Interlocked.Increment(ref _packetId), PacketType = PacketType.CommandFailure, Parameter = parameter };

    /// <inheritdoc/>
    public PipeMessage CreateResponse(PipeMessage commandMessage) 
      => new() { Command = commandMessage.Command, Id = commandMessage.Id, PacketType = PacketType.Response };

    /// <inheritdoc/>
    public PipeMessage CreateResponse(PipeMessage commandMessage, object? parameter)
      => new() { Command = commandMessage.Command, Id = commandMessage.Id, PacketType = PacketType.Response, Parameter = parameter };

    /// <inheritdoc/>
    public PipeMessage CreateResponseCancellation(PipeMessage commandMessage, Exception? parameter = default)
      => new() { Command = commandMessage.Command, Id = commandMessage.Id, PacketType = PacketType.ResponseCancellation, Parameter = parameter };

    /// <inheritdoc/>
    public PipeMessage CreateResponseFailure(PipeMessage commandMessage, Exception parameter)
      => new() { Command = commandMessage.Command, Id = commandMessage.Id, PacketType = PacketType.ResponseFailure, Parameter = parameter };
  }
}
