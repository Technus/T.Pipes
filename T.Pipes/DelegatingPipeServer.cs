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

    public T? InvokeRemote<T>(object? parameters, [CallerMemberName] string callerName = "") => Callback.Remote<T, object>(callerName, parameters);

    public T? InvokeRemote<T>([CallerMemberName] string callerName = "") => Callback.Remote<T>(callerName);

    public void InvokeRemote(object? parameters, [CallerMemberName] string callerName = "") => Callback.Remote(callerName, parameters);

    public void InvokeRemote([CallerMemberName] string callerName = "") => Callback.Remote(callerName);

    public T? GetRemote<T>([CallerMemberName] string callerName = "") => Callback.Remote<T>(callerName);

    public void SetRemote<T>(T? value, [CallerMemberName] string callerName = "") => Callback.Remote(callerName, value);

    public void SetFunctionRemote(Func<object?, object?> function, string callerName) => Callback.SetFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}
