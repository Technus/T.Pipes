using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClientCallback<TTarget>
    : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, TTarget>
  {
    public DelegatingPipeClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }
  }

  public class DelegatingPipeServerCallback<TTarget>
    : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, TTarget>
  {
    public DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  public class DelegatingPipeCallback<TPipe, TTarget>
    : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
    where TPipe : H.Pipes.IPipeConnection<PipeMessage>
  {
    public DelegatingPipeCallback(TPipe pipe) : base(pipe, new())
    {
    }

    public DelegatingPipeCallback(TPipe pipe, TTarget target) : base(pipe, new(), target)
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
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _responses = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, Func<object?, object?>> _functions = [];
    private TTarget _target;

    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
      if (this is TTarget tt)
      {
        Target = tt;
      }

      if (_target is null)
      {
        throw new InvalidOperationException($"In fact this is not {typeof(TTarget).FullName}");
      }
    }

    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory, TTarget target)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
      if (target is not null)
      {
        Target = target;
      }

      if (_target is null)
      {
        throw new InvalidOperationException($"In fact target is not {typeof(TTarget).FullName}");
      }
    }

    public TTarget Target
    {
      get => _target;
      private set
      {
        if (Target is not null)
        {
          TargetDeInitAuto();
          TargetDeInit(Target);
        }

        _target = value;

        if (Target is not null)
        {
          TargetInitAuto();
          TargetInit(Target);
        }
      }
    }

    protected virtual void TargetInit(TTarget target) { }

    protected virtual void TargetDeInit(TTarget target) { }

    protected virtual void TargetInitAuto() { }

    protected virtual void TargetDeInitAuto() { }

    protected TPipe Pipe { get; }
    protected TPacketFactory PacketFactory { get; }

    public Task ConnectedOnce => _connectedOnce.Task;
    public Task FailedOnce => _failedOnce.Task;

    public virtual void Connected(string connection)
    {
      Clear();
      _ = _connectedOnce.TrySetResult(null);
    }

    public virtual void Disconnected(string connection)
    {
      Clear();
      _ = _failedOnce.TrySetResult(null);
      _ = _connectedOnce.TrySetCanceled();
    }

    public void Dispose()
    {
      DisposeAsync().AsTask().Wait();
    }

    public virtual async ValueTask DisposeAsync()
    {
      await _semaphore.WaitAsync();
      foreach (KeyValuePair<Guid, TaskCompletionSource<object?>> item in _responses)
      {
        _ = item.Value.TrySetCanceled();
      }
      _responses.Clear();
      if (_target is not null)
      {
        TargetDeInitAuto();
        TargetDeInit(_target);
      }
      _functions.Clear();
      _ = _failedOnce.TrySetCanceled();
      _ = _connectedOnce.TrySetCanceled();
      _semaphore.Dispose();
    }

    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (KeyValuePair<Guid, TaskCompletionSource<object?>> item in _responses)
      {
        _ = item.Value.TrySetCanceled();
      }
      _responses.Clear();
      _ = _semaphore.Release();
    }

    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear();
      _ = _failedOnce.TrySetResult(null);
      _ = _connectedOnce.TrySetException(e);
    }

    public void OnMessageReceived(TPacket? message)
    {
      if (message is null)
      {
        return;
      }

      if (_responses.TryGetValue(message.Id, out TaskCompletionSource<object?>? response))
      {
        _ = response.TrySetResult(message.Parameter);
        _semaphore.Wait();
        _ = _responses.Remove(message.Id);
        _ = _semaphore.Release();
      }
      else
      {
        OnCommandReceived(message);
      }
    }

    protected virtual void OnCommandReceived(TPacket message)
    {
      if (_functions.TryGetValue(message.Command, out Func<object?, object?>? function))
      {
        _ = Pipe.WriteAsync(PacketFactory.CreateResponse(message, function.Invoke(message.Parameter)));
      }
      else if (OnCommandReceivedAuto(message))
      {
        return;
      }
      OnUnknownCommand(message);
    }

    protected virtual bool OnCommandReceivedAuto(TPacket message)
    {
      return false;
    }

    protected virtual void OnUnknownCommand(TPacket message)
    {
      throw new ArgumentException($"Message is unknown: {message}", nameof(message));
    }

    public virtual void OnMessageSent(TPacket? message) { }

#nullable disable
    public T GetResponse<T>(TPacket message)
    {
      TaskCompletionSource<object> tcs = new();
      _semaphore.Wait();
      _responses.Add(message.Id, tcs);
      _ = _semaphore.Release();
      return (T)tcs.Task.Result;
    }

    public async Task<T> GetResponseAsync<T>(TPacket message)
    {
      TaskCompletionSource<object> tcs = new();
      await _semaphore.WaitAsync();
      _responses.Add(message.Id, tcs);
      _ = _semaphore.Release();
      return (T)await tcs.Task;
    }
#nullable restore

    public void SendResponse(TPacket message)
    {
      TPacket response = PacketFactory.CreateResponse(message);
      Pipe.WriteAsync(response).Wait();
    }

    public async Task SendResponseAsync(TPacket message)
    {
      TPacket response = PacketFactory.CreateResponse(message);
      await Pipe.WriteAsync(response);
    }

    public void SendResponse<T>(TPacket message, T parameter)
    {
      TPacket response = PacketFactory.CreateResponse(message, parameter);
      Pipe.WriteAsync(response).Wait();
    }

    public async Task SendResponseAsync<T>(TPacket message, T parameter)
    {
      TPacket response = PacketFactory.CreateResponse(message, parameter);
      await Pipe.WriteAsync(response);
    }

    public async Task<TOut> RemoteAsync<TOut>(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    public async Task<TOut> RemoteAsync<TIn, TOut>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    public async Task RemoteAsync(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      _ = await GetResponseAsync<object?>(cmd);
    }

    public async Task RemoteAsync<TIn>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      _ = await GetResponseAsync<object?>(cmd);
    }

    public TOut Remote<TOut>(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
    }

    public TOut Remote<TIn, TOut>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
    }

    public void Remote(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    public void SetFunction(string callerName, Func<object?, object?> function)
    {
      _functions[callerName] = function;
    }

    public void RemoveFunction(string callerName)
    {
      _ = _functions.Remove(callerName);
    }
  }
}
