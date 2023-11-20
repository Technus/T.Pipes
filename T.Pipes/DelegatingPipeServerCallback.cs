using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeServerCallback<TPacket, TPacketFactory>
    : DelegatingPipeCallback<IPipeServer<TPacket>, TPacket, TPacketFactory>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public DelegatingPipeServerCallback(IPipeServer<TPacket> pipe, TPacketFactory packetFactory)
      : base(pipe, packetFactory)
    {
    }
  }
}
