using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClientCallback<TTarget, TPacket, TPacketFactory> : IPipeCallback<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    private readonly TaskCompletionSource<object?> _failedOnce = new();
    public Task FailedOnce => _failedOnce.Task;

    private readonly IDictionary<Guid, TaskCompletionSource<object?>> _responses = new Dictionary<Guid, TaskCompletionSource<object?>>();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly IDictionary<string, Func<object?, object?>> _functions = new Dictionary<string, Func<object?, object?>>();

    private readonly IPipeClient<TPacket> _pipe;

    private readonly TPacketFactory _packetFactory;
    public TTarget Target { get; }
    private readonly Type _type;
    private readonly IDictionary<string, Delegate> _reflectionCache = new Dictionary<string, Delegate>();

    internal DelegatingPipeClientCallback(TPacketFactory packetFactory, TTarget target, IPipeClient<TPacket> pipe)
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

      if (message is null)
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
}