﻿using H.Pipes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClientCallback<TTarget, TPacket, TPacketFactory> 
    : DelegatingPipeCallback<IPipeClient<TPacket>, TPacket, TPacketFactory>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public TTarget Target { get; }
    private readonly Type _type;

    public DelegatingPipeClientCallback(TPacketFactory packetFactory, TTarget target, IPipeClient<TPacket> pipe)
      : base(pipe, packetFactory)
    {
      Target = target;
      _type = target?.GetType() ?? typeof(TTarget);
    }
  }
}