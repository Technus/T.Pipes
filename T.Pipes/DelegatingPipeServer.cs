using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeServer<TTarget>
    : DelegatingPipeServer<TTarget, DelegatingPipeServerCallback<TTarget>>
  {
    public DelegatingPipeServer(string pipe, DelegatingPipeServerCallback<TTarget> callback) : base(pipe, callback)
    {
    }

    public DelegatingPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, DelegatingPipeServerCallback<TTarget> callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeServer<TTarget, TCallback>
    : DelegatingPipeServer<H.Pipes.PipeServer<PipeMessage>, TTarget, TCallback>
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public DelegatingPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeServer<TPipe, TTarget, TCallback>
    : DelegatingPipeServer<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeServer(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeServer<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeServer<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget>
  {
    public TTarget Target => Callback.Target;

    public DelegatingPipeServer(TPipe pipe, TCallback callback)
      : base(pipe, callback)
    {
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public T? InvokeRemote<T>(object? parameters, [CallerMemberName] string callerName = "")
    {
      return Callback.Remote<object, T>(callerName, parameters);
    }

    public T? InvokeRemote<T>([CallerMemberName] string callerName = "")
    {
      return Callback.Remote<T>(callerName);
    }

    public void InvokeRemote(object? parameters, [CallerMemberName] string callerName = "")
    {
      Callback.Remote(callerName, parameters);
    }

    public void InvokeRemote([CallerMemberName] string callerName = "")
    {
      Callback.Remote(callerName);
    }

    public async Task<T?> InvokeRemoteAsync<T>(object? parameters, [CallerMemberName] string callerName = "")
    {
      return await Callback.RemoteAsync<object, T>(callerName, parameters);
    }

    public async Task<T?> InvokeRemoteAsync<T>([CallerMemberName] string callerName = "")
    {
      return await Callback.RemoteAsync<T>(callerName);
    }

    public async Task InvokeRemoteAsync(object? parameters, [CallerMemberName] string callerName = "")
    {
      await Callback.RemoteAsync(callerName, parameters);
    }

    public async Task InvokeRemoteAsync([CallerMemberName] string callerName = "")
    {
      await Callback.RemoteAsync(callerName);
    }

    public T? GetRemote<T>([CallerMemberName] string callerName = "")
    {
      return Callback.Remote<T>(callerName);
    }

    public void SetRemote<T>(T? value, [CallerMemberName] string callerName = "")
    {
      Callback.Remote(callerName, value);
    }

    public async Task<T?> GetRemoteAsync<T>([CallerMemberName] string callerName = "")
    {
      return await Callback.RemoteAsync<T>(callerName);
    }

    public async Task SetRemoteAsync<T>(T? value, [CallerMemberName] string callerName = "")
    {
      await Callback.RemoteAsync(callerName, value);
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
