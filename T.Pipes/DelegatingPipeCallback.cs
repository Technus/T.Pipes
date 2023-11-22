using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeMessageCallback<TPipe, TTarget> : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
    where TPipe : H.Pipes.IPipeConnection<PipeMessage>
  {
    public DelegatingPipeMessageCallback(TPipe pipe, TTarget? target = default) : base(pipe, new(), target)
    {
    }
  }

  public class DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget> 
    : IPipeCallback<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    private readonly TaskCompletionSource<object?> _failedOnce = new();
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _responses = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, Func<object?, object?>> _functions = new();
    private TTarget? _target;

    public TTarget? Target
    {
      get => _target;
      set
      {
        _target = value;
        Type = value?.GetType() ?? typeof(TTarget);
      }
    }

    public Type Type { get; private set; }

    protected TPipe Pipe { get; }
    protected TPacketFactory PacketFactory { get; }

    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory, TTarget? target = default)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
      _target = target;
      Type = target?.GetType() ?? typeof(TTarget);
    }

    public Task ConnectedOnce => _connectedOnce.Task;
    public Task FailedOnce => _failedOnce.Task;

    public virtual void Connected(string connection)
    {
      Clear();
      _connectedOnce.TrySetResult(null);
    }

    public virtual void Disconnected(string connection)
    {
      Clear();
      _failedOnce.TrySetResult(null);
      _connectedOnce.TrySetCanceled();
    }

    public void Dispose() => DisposeAsync().AsTask().Wait();

    public virtual async ValueTask DisposeAsync()
    {
      await _semaphore.WaitAsync();
      foreach (var item in _responses)
      {
        item.Value.TrySetCanceled();
      }
      _responses.Clear();
      _functions.Clear();
      _failedOnce.TrySetCanceled();
      _connectedOnce.TrySetCanceled();
      _semaphore.Dispose();
    }

    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (var item in _responses)
      {
        item.Value.TrySetCanceled();
      }
      _responses.Clear();
      _semaphore.Release();
    }

    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear();
      _failedOnce.TrySetResult(null);
      _connectedOnce.TrySetException(e);
    }

    public void OnMessageReceived(TPacket? message)
    {
      if (message is null)
      {
        return;
      }

      if (_responses.TryGetValue(message.Id, out var response))
      {
        response.TrySetResult(message.Parameter);
        _semaphore.Wait();
        _responses.Remove(message.Id);
        _semaphore.Release();
      }
      else 
      {
        OnCommandReceived(message);
      }
    }

    protected virtual void OnCommandReceived(TPacket message)
    {
      if (_functions.TryGetValue(message.Command, out var function))
      {
        Pipe.WriteAsync(PacketFactory.CreateResponse(message, function.Invoke(message.Parameter)));
      }
      else if(OnAutoCommand(message))
      {
        return;
      }
      OnUnknownCommand(message);
    }

    protected virtual bool OnAutoCommand(TPacket message) => false;
    protected virtual void OnUnknownCommand(TPacket message) => throw new ArgumentException($"Message is unknown: {message}", nameof(message));

    public virtual void OnMessageSent(TPacket? message) { }

#nullable disable
    public T GetResponse<T>(TPacket message)
    {
      var tcs = new TaskCompletionSource<object>();
      _semaphore.Wait();
      _responses.Add(message.Id, tcs);
      _semaphore.Release();
      return (T) tcs.Task.Result;
    }

    public async Task<T> GetResponseAsync<T>(TPacket message)
    {
      var tcs = new TaskCompletionSource<object>();
      await _semaphore.WaitAsync();
      _responses.Add(message.Id, tcs);
      _semaphore.Release();
      return (T) await tcs.Task;
    }
#nullable restore

    public void SendResponse(TPacket message)
    {
      var response = PacketFactory.CreateResponse(message);
      Pipe.WriteAsync(response).Wait();
    }

    public async Task SendResponseAsync(TPacket message)
    {
      var response = PacketFactory.CreateResponse(message);
      await Pipe.WriteAsync(response);
    }

    public void SendResponse<T>(TPacket message, T parameter)
    {
      var response = PacketFactory.CreateResponse(message, parameter);
      Pipe.WriteAsync(response).Wait();
    }

    public async Task SendResponseAsync<T>(TPacket message, T parameter)
    {
      var response = PacketFactory.CreateResponse(message, parameter);
      await Pipe.WriteAsync(response);
    }

    public async Task<TOut> RemoteAsync<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    public async Task<TOut> RemoteAsync<TIn, TOut>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    public async Task RemoteAsync(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      await GetResponseAsync<object?>(cmd);
    }

    public async Task RemoteAsync<TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      await GetResponseAsync<object?>(cmd);
    }

    public TOut Remote<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
    }

    public TOut Remote<TIn, TOut>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
    }

    public void Remote(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    public void SetFunction(string callerName, Func<object?, object?> function) => _functions[callerName] = function;

    public void RemoveFunction(string callerName) => _functions.Remove(callerName);
  }
}
