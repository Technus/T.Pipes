using System;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public partial class DelegatingPipeClient<TTarget,TPacket,TPacketFactory> 
    : PipeClient<TPacket, DelegatingPipeClientCallback<TTarget, TPacket, TPacketFactory>> 
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public TPacketFactory PacketFactory { get; }

    public TTarget Target => Callback.Target;

    public DelegatingPipeClient(TPacketFactory packetFactory, string pipeName, TTarget target) 
      : this(packetFactory, new PipeClient<TPacket>(pipeName), target)
    {
    }

    public DelegatingPipeClient(TPacketFactory packetFactory, IPipeClient<TPacket> pipe, TTarget target) 
      : base(pipe, new DelegatingPipeClientCallback<TTarget, TPacket, TPacketFactory>(packetFactory, target, pipe))
    {
      PacketFactory = packetFactory;
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public void EventRemote(object? parameters, string callerName)
    {
      var cmd = PacketFactory.Create(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      Callback.GetResponse(cmd);
    }

    public void EventRemote(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      Callback.GetResponse(cmd);
    }

    public T? EventRemote<T>(object? parameters, string callerName)
    {
      var cmd = PacketFactory.Create(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd);
    }

    public T? EventRemote<T>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd);
    }

    public void SetFunctionRemote(Func<object?, object?> function, string callerName) => Callback.SetFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}