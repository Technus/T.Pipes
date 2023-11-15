using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using Idefix.BaseClasses.Patterns;

namespace T.Pipes
{
  /// <summary>
  /// IPC helper for remote interface consumer side
  /// </summary>
  public class DelegatingPipeServer : PipeServer<PipeMessage, DelegatingPipeServer.PipeCallback>
  {

    public DelegatingPipeServer(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingPipeServer(IPipeServer<PipeMessage> pipe) : base(pipe, new PipeCallback(pipe))
    {
    }

    public class PipeCallback : DisposableAsync, IPipeCallback<PipeMessage>
    {
      private readonly TaskCompletionSource<object?> _connectedOnce = new TaskCompletionSource<object?>();
      private readonly TaskCompletionSource<object?> _failedOnce = new TaskCompletionSource<object?>();
      public Task ConnectedOnce => _connectedOnce.Task;
      public Task FailedOnce => _failedOnce.Task;

      private readonly IDictionary<Guid, TaskCompletionSource<object?>> _responses = new Dictionary<Guid, TaskCompletionSource<object?>>();
      private readonly SemaphoreSlim _semaphore = new(1, 1);

      private readonly IDictionary<string, Action<object?>> _events = new Dictionary<string, Action<object?>>();

      private readonly IPipeServer<PipeMessage> _pipe;

      internal PipeCallback(IPipeServer<PipeMessage> pipe) => _pipe = pipe;

      protected override async ValueTask DisposeManagedAsync()
      {
        await base.DisposeManagedAsync();
        _events.Clear();
        _semaphore.Wait();
        foreach (var item in _responses)
        {
          item.Value.SetCanceled();
        }
        _responses.Clear();
        _semaphore.Dispose();
        _connectedOnce.TrySetCanceled();
        _failedOnce.TrySetCanceled();
      }

      public void Clear()
      {
        _semaphore.Wait();
        foreach (var item in _responses)
        {
          item.Value.TrySetCanceled();
        }
        _responses.Clear();
        _semaphore.Release();
      }

      public void Connected(string connection)
      {
        Debug.Write("Connected ");
        Debug.WriteLine(connection);
        Clear();
        _connectedOnce.TrySetResult(null);
      }

      public void Disconnected(string connection)
      {
        Debug.Write("Disconnected ");
        Debug.WriteLine(connection);
        Clear();
        _failedOnce.TrySetResult(null);
        _connectedOnce.TrySetCanceled();
      }

      public void OnExceptionOccurred(Exception e)
      {
        Debug.WriteLine(e);
        Clear();
        _failedOnce.TrySetResult(null);
        _connectedOnce.TrySetException(e);
      }

      public void OnMessageReceived(PipeMessage? message)
      {
        Debug.WriteLine(message);

        if (message == null)
        {
          return;
        }

        if (_events.TryGetValue(message.Command, out var action))
        {
          action.Invoke(message.Parameter);
          _pipe.WriteAsync(message.ToResponse<object>()).Wait();
        }
        else
        {
          if (_responses.TryGetValue(message.Id, out var response))
          {
            response.TrySetResult(message.Parameter);
            _semaphore.Wait();
            _responses.Remove(message.Id);
            _semaphore.Release();
          }
        }
      }

      public void OnMessageSent(PipeMessage? message)
      {
        Debug.WriteLine(message);
      }

      public Task<object?> GetResponse(PipeMessage message)
      {
        var tcs = new TaskCompletionSource<object?>();
        _semaphore.Wait();
        _responses.Add(message.Id, tcs);
        _semaphore.Release();
        return tcs.Task;
      }

      public void AddAction(string callerName, Action<object?> action) => _events[callerName] = action;
      public void RemoveAction(string callerName) => _events.Remove(callerName);
    }

    protected override async ValueTask DisposeManagedAsync()
    {
      await base.DisposeManagedAsync();
      await Callback.DisposeAsync();
    }

    public T? InvokeRemote<T>(object[] parameters, [CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd).Result;
    }

    public T? InvokeRemote<T>([CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd).Result;
    }

    public void InvokeRemote(object[] parameters, [CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }

    public void InvokeRemote([CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }

    public T? GetRemote<T>([CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage("get_" + callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd).Result;
    }

    public void SetRemote<T>(T? value, [CallerMemberName] string callerName = "")
    {
      var cmd = new PipeMessage("set_" + callerName, value);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }

    public void AddEventRemote(Action<object?> action, string callerName)
    {
      Callback.AddAction(callerName, action);
    }

    public void RemoveEventRemote(string callerName)
    {
      Callback.RemoveAction(callerName);
    }
  }
}
