using System;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public partial class DelegatingPipeClient<TPacket, TPacketFactory, TTarget> 
    : PipeClient<TPacket, DelegatingPipeCallback<IPipeClient<TPacket>, TPacket, TPacketFactory, TTarget>> 
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public DelegatingPipeClient(string pipeName, TPacketFactory packetFactory, TTarget? target = default) 
      : this(new PipeClient<TPacket>(pipeName), packetFactory, target)
    {
    }

    public DelegatingPipeClient(IPipeClient<TPacket> pipe, TPacketFactory packetFactory, TTarget? target = default)
      : base(pipe, new(pipe, packetFactory, target))
    {
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public void EventRemote(object? parameters, string callerName) => Callback.Remote(callerName, parameters);

    public void EventRemote(string callerName) => Callback.Remote(callerName);

    public T? EventRemote<T>(object? parameters, string callerName) => Callback.Remote<T, object>(callerName, parameters);

    public T? EventRemote<T>(string callerName) => Callback.Remote<T>(callerName);

    public async Task EventRemoteAsync(object? parameters, string callerName) => await Callback.RemoteAsync(callerName, parameters);

    public async Task EventRemoteAsync(string callerName) => await Callback.RemoteAsync(callerName);

    public async Task<T?> EventRemoteAsync<T>(object? parameters, string callerName) => await Callback.RemoteAsync<T, object>(callerName, parameters);

    public async Task<T?> EventRemoteAsync<T>(string callerName) => await Callback.RemoteAsync<T>(callerName);

    public void SetFunctionRemote(Func<object?, object?> function, string callerName) => Callback.SetFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}