using System;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClient<TTarget, TCallback>
      : DelegatingPipeClient<H.Pipes.PipeClient<PipeMessage>, TTarget, TCallback>
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public DelegatingPipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeClient<TPipe, TTarget, TCallback>
    : DelegatingPipeClient<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TPipe : H.Pipes.IPipeClient<PipeMessage>
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeClient<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeClient<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget>
  {
    public TTarget Target => Callback.Target;

    public DelegatingPipeClient(TPipe pipe, TCallback callback)
      : base(pipe, callback)
    {
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public void EventRemote(object? parameters, string callerName)
    {
      Callback.Remote(callerName, parameters);
    }

    public void EventRemote(string callerName)
    {
      Callback.Remote(callerName);
    }

    public T? EventRemote<T>(object? parameters, string callerName)
    {
      return Callback.Remote<object, T>(callerName, parameters);
    }

    public T? EventRemote<T>(string callerName)
    {
      return Callback.Remote<T>(callerName);
    }

    public async Task EventRemoteAsync(object? parameters, string callerName)
    {
      await Callback.RemoteAsync(callerName, parameters);
    }

    public async Task EventRemoteAsync(string callerName)
    {
      await Callback.RemoteAsync(callerName);
    }

    public async Task<T?> EventRemoteAsync<T>(object? parameters, string callerName)
    {
      return await Callback.RemoteAsync<object, T>(callerName, parameters);
    }

    public async Task<T?> EventRemoteAsync<T>(string callerName)
    {
      return await Callback.RemoteAsync<T>(callerName);
    }

    public void SetFunctionRemote(Func<object?, object?> function, string callerName)
    {
      Callback.SetFunction(callerName, function);
    }

    public void RemoveFunctionRemote(string callerName)
    {
      Callback.RemoveFunction(callerName);
    }
  }
}