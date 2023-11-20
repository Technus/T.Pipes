using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// IPC helper for remote interface consumer side
  /// </summary>
  public partial class DelegatingPipeServer<TPacket, TPacketFactory> 
    : PipeServer<TPacket, DelegatingPipeServerCallback<TPacket, TPacketFactory>> 
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public TPacketFactory PacketFactory { get; }

    public DelegatingPipeServer(TPacketFactory packetFactory, string pipeName) 
      : this(packetFactory, new PipeServer<TPacket>(pipeName))
    {
    }

    public DelegatingPipeServer(TPacketFactory packetFactory, IPipeServer<TPacket> pipe) 
      : base(pipe, new DelegatingPipeServerCallback<TPacket, TPacketFactory>(pipe, packetFactory))
    {
      PacketFactory = packetFactory;
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public T? InvokeRemote<T>(object? parameters, [CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd);
    }

    public T? InvokeRemote<T>([CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd);
    }

    public void InvokeRemote(object? parameters, [CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd);
    }

    public void InvokeRemote([CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd);
    }

    public T? GetRemote<T>([CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create("get_" + callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd);
    }

    public void SetRemote<T>(T? value, [CallerMemberName] string callerName = "")
    {
      var cmd = PacketFactory.Create("set_" + callerName, value);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd);
    }

    public void SetFunctionRemote(Func<object?, object?> function, string callerName) => Callback.SetFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}
