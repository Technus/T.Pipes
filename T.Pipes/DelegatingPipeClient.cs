using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClient<TTarget,TPacket,TPacketFactory> : PipeClient<TPacket, DelegatingPipeClient<TTarget, TPacket, TPacketFactory>.PipeCallback> 
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    public TPacketFactory PacketFactory { get; }

    protected TTarget Target => Callback.Target;

    public DelegatingPipeClient(TPacketFactory packetFactory, string pipeName, TTarget target) : this(packetFactory, new PipeClient<TPacket>(pipeName), target)
    {
    }

    public DelegatingPipeClient(TPacketFactory packetFactory, IPipeClient<TPacket> pipe, TTarget target) : base(pipe, new PipeCallback(packetFactory, target, pipe))
    {
      PacketFactory = packetFactory;
    }

    public class PipeCallback : IPipeCallback<TPacket>
    {
      private readonly TaskCompletionSource<object?> _failedOnce = new();
      public Task FailedOnce => _failedOnce.Task;

      private readonly IDictionary<Guid, TaskCompletionSource<object?>> _responses = new Dictionary<Guid, TaskCompletionSource<object?>>();
      private readonly SemaphoreSlim _semaphore = new(1, 1);

      private readonly IDictionary<string, Func<object?,object?>> _functions = new Dictionary<string, Func<object?, object?>>();

      private readonly IPipeClient<TPacket> _pipe;

      private readonly TPacketFactory _packetFactory;
      public TTarget Target { get; }
      private readonly Type _type;
      private readonly IDictionary<string, Delegate> _reflectionCache = new Dictionary<string, Delegate>();

      internal PipeCallback(TPacketFactory packetFactory, TTarget target, IPipeClient<TPacket> pipe)
      {
        _packetFactory = packetFactory;
        Target = target;
        _pipe = pipe;
        _type = target?.GetType() ?? typeof(TTarget);
      }

      public async ValueTask DisposeAsync()
      {
        _functions.Clear();
        await _semaphore.WaitAsync();
        foreach (var item in _responses)
        {
          item.Value.TrySetCanceled();
        }
        _responses.Clear();
        _semaphore.Dispose();
        _failedOnce.TrySetCanceled();
      }

      public void Dispose() => DisposeAsync().AsTask().Wait();

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
      }

      public void Disconnected(string connection)
      {
        Debug.Write("Disconnected ");
        Debug.WriteLine(connection);
        Clear();
        _failedOnce.TrySetResult(null);
      }

      public void OnExceptionOccurred(Exception e)
      {
        Debug.WriteLine(e);
        Clear();
        _failedOnce.TrySetResult(null);
      }

      public void OnMessageReceived(TPacket? message)
      {
        Debug.WriteLine(message);

        if(message is null)
        {
          return;
        }

        if (_functions.TryGetValue(message.Command, out var action))
        {
          _pipe.WriteAsync(_packetFactory.CreateResponse(message, action.Invoke(message.Parameter))).Wait();
        }
        else
        {
          if (_responses.TryGetValue(message.Id, out var response))
          {
            response.TrySetResult(null);
            _semaphore.Wait();
            _responses.Remove(message.Id);
            _semaphore.Release();
          }

          var target = message.Command;
          bool setter = target.StartsWith("set_");
          bool getter = target.StartsWith("get_");
          if (setter || getter)
          {
            target = target.Substring(4);
            var property = _type.GetProperty(target);
            if (property is null)
            {
              throw new InvalidOperationException($"Property {target} on type {_type.Name} does not exist.");
            }
            if (setter)
            {
              property.SetValue(Target, message.Parameter);
              _pipe.WriteAsync(_packetFactory.CreateResponse(message)).Wait();
            }
            else
            {
              var value = property.GetValue(Target);
              _pipe.WriteAsync(_packetFactory.CreateResponse(message, value)).Wait();
            }
            return;
          }

          var method = _type.GetMethod(target);
          if (method is null)
          {
            throw new InvalidOperationException($"Method {target} on type {_type.Name} does not exist.");
          }
          var returned = method.Invoke(Target, message.Parameter as object?[]);
          _pipe.WriteAsync(_packetFactory.CreateResponse(message, returned)).Wait();
        }
      }

      public Task<object?> GetResponse(TPacket message)
      {
        var tcs = new TaskCompletionSource<object?>();
        _semaphore.Wait();
        _responses.Add(message.Id, tcs);
        _semaphore.Release();
        return tcs.Task;
      }

      public void OnMessageSent(TPacket? message) => Debug.WriteLine(message);

      public void AddFunction(string callerName, Func<object?, object?> function) => _functions[callerName] = function;
      public void RemoveFunction(string callerName) => _functions.Remove(callerName);
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
      _ = Callback.GetResponse(cmd).Result;
    }

    public void EventRemote(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }

    public T? EventRemote<T>(object? parameters, string callerName)
    {
      var cmd = PacketFactory.Create(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd).Result;
    }

    public T? EventRemote<T>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (T?)Callback.GetResponse(cmd).Result;
    }

    public void AddFunctionRemote(Func<object?, object?> function, string callerName) => Callback.AddFunction(callerName, function);

    public void RemoveFunctionRemote(string callerName) => Callback.RemoveFunction(callerName);
  }
}