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

      public Task<object?> GetResponse(TPacket message)
      {
        var tcs = new TaskCompletionSource<object?>();
        _semaphore.Wait();
        _responses.Add(message.Id, tcs);
        _semaphore.Release();
        return tcs.Task;
      }

      public void OnMessageSent(TPacket? message)
      {
        Debug.WriteLine(message);
      }

      public void AddProperty(string propertyName, PropertyInfo? propertyInfo = null)
      {
        propertyInfo ??= _type.GetProperty(propertyName) ?? throw new ArgumentNullException();
        var type = propertyInfo.PropertyType;
        var setter = propertyInfo.GetSetMethod()?.CreateDelegate(typeof(Action<>).MakeGenericType(type), Target);
        if (setter != null)
        {
          _reflectionCache.Add("set_" + propertyName, setter);
        }
        var getter = propertyInfo.GetGetMethod()?.CreateDelegate(typeof(Func<>).MakeGenericType(type), Target);
        if (getter != null)
        {
          _reflectionCache.Add("get_" + propertyName, getter);
        }
      }

      public void AddMethod(string methodName, MethodInfo? methodInfo = null)
      {
        methodInfo ??= _type.GetMethod(methodName) ?? throw new ArgumentNullException();
        var returnType = methodInfo.ReturnType;
        var parameters = methodInfo.GetParameters();
        var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();
        if (returnType == typeof(void))
        {
          var type = parameterTypes.Length switch
          {
            0 => typeof(Action),
            1 => typeof(Action<>),
            2 => typeof(Action<,>),
            3 => typeof(Action<,,>),
            4 => typeof(Action<,,,>),
            5 => typeof(Action<,,,,>),
            6 => typeof(Action<,,,,,>),
            7 => typeof(Action<,,,,,,>),
            8 => typeof(Action<,,,,,,,>),
            9 => typeof(Action<,,,,,,,,>),
            10 => typeof(Action<,,,,,,,,,>),
            11 => typeof(Action<,,,,,,,,,,>),
            12 => typeof(Action<,,,,,,,,,,,>),
            13 => typeof(Action<,,,,,,,,,,,,>),
            14 => typeof(Action<,,,,,,,,,,,,,>),
            15 => typeof(Action<,,,,,,,,,,,,,,>),
            16 => typeof(Action<,,,,,,,,,,,,,,,>),
            _ => throw new ArgumentNullException(),
          };
          if (parameterTypes.Length == 0)
          {
            var action = methodInfo.CreateDelegate(type, Target);
            _reflectionCache.Add(methodName, action);
          }
          else if (type != null)
          {
            var action = methodInfo.CreateDelegate(type.MakeGenericType(parameterTypes), Target);
            _reflectionCache.Add(methodName, action);
          }
        }
        else
        {
          var type = parameterTypes.Length switch
          {
            0 => typeof(Func<>),
            1 => typeof(Func<,>),
            2 => typeof(Func<,,>),
            3 => typeof(Func<,,,>),
            4 => typeof(Func<,,,,>),
            5 => typeof(Func<,,,,,>),
            6 => typeof(Func<,,,,,,>),
            7 => typeof(Func<,,,,,,,>),
            8 => typeof(Func<,,,,,,,,>),
            9 => typeof(Func<,,,,,,,,,>),
            10 => typeof(Func<,,,,,,,,,,>),
            11 => typeof(Func<,,,,,,,,,,,>),
            12 => typeof(Func<,,,,,,,,,,,,>),
            13 => typeof(Func<,,,,,,,,,,,,,>),
            14 => typeof(Func<,,,,,,,,,,,,,,>),
            15 => typeof(Func<,,,,,,,,,,,,,,,>),
            16 => typeof(Func<,,,,,,,,,,,,,,,,>),
            _ => throw new ArgumentNullException(),
          };
          if (type != null)
          {
            var function = methodInfo.CreateDelegate(type.MakeGenericType(parameterTypes), Target);
            _reflectionCache.Add(methodName, function);
          }
        }
      }
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public void EventRemote(object[] parameters, string callerName)
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

    public T? EventRemote<T>(object[] parameters, string callerName)
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
  }
}