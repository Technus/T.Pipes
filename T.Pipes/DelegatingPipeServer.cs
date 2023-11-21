using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeServer<TPacket, TPacketFactory, TTarget> 
    : DelegatingPipeServer<TPacket, TPacketFactory, TTarget, DelegatingPipeCallback<IPipeServer<TPacket>, TPacket, TPacketFactory, TTarget>>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public DelegatingPipeServer(string pipeName, TPacketFactory packetFactory)
      : this(new PipeServer<TPacket>(pipeName), packetFactory, default)
    {
      if (this is TTarget target)
      {
        Callback.Target = target;
      }
    }

    public DelegatingPipeServer(IPipeServer<TPacket> pipe, TPacketFactory packetFactory)
      : base(pipe, new(pipe, packetFactory, default))
    {
      if (this is TTarget target)
      {
        Callback.Target = target;
      }
    }

    public DelegatingPipeServer(string pipeName, TPacketFactory packetFactory, TTarget? target)
      : this(new PipeServer<TPacket>(pipeName), packetFactory, target)
    {
    }

    public DelegatingPipeServer(IPipeServer<TPacket> pipe, TPacketFactory packetFactory, TTarget? target)
      : base(pipe, new(pipe, packetFactory, target))
    {
    }
  }

  public class DelegatingPipeServer<TPacket, TPacketFactory, TTarget, TCallback> 
    : PipeServer<TPacket, TCallback> 
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<IPipeServer<TPacket>, TPacket, TPacketFactory, TTarget>
  {
    public DelegatingPipeServer(string pipeName, TCallback callback)
      : this(new PipeServer<TPacket>(pipeName), callback)
    {
    }

    public DelegatingPipeServer(IPipeServer<TPacket> pipe, TCallback callback)
      : base(pipe, callback)
    {
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

    public async Task<T?> InvokeRemoteAsync<T>(object? parameters, [CallerMemberName] string callerName = "") => await Callback.RemoteAsync<T, object>(callerName, parameters);

    public async Task<T?> InvokeRemoteAsync<T>([CallerMemberName] string callerName = "") => await Callback.RemoteAsync<T>(callerName);

    public async Task InvokeRemoteAsync(object? parameters, [CallerMemberName] string callerName = "") => await Callback.RemoteAsync(callerName, parameters);

    public async Task InvokeRemoteAsync([CallerMemberName] string callerName = "") => await Callback.RemoteAsync(callerName);

    public T? GetRemote<T>([CallerMemberName] string callerName = "") => Callback.Remote<T>(callerName);

    public void SetRemote<T>(T? value, [CallerMemberName] string callerName = "") => Callback.Remote(callerName, value);

    public async Task<T?> GetRemoteAsync<T>([CallerMemberName] string callerName = "") => await Callback.RemoteAsync<T>(callerName);

    public async Task SetRemoteAsync<T>(T? value, [CallerMemberName] string callerName = "") => await Callback.RemoteAsync(callerName, value);

    public void SetFunctionRemote(Func<object?, object?> function, string callerName) => Callback.SetFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}
