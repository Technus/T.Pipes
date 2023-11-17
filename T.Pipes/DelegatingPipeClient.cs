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
  public class DelegatingPipeClient : PipeClient<PipeMessage, DelegatingPipeClient.PipeCallback>
  {
    public DelegatingPipeClient(string pipeName, object target) : this(new PipeClient<PipeMessage>(pipeName), target)
    {
    }

    public DelegatingPipeClient(IPipeClient<PipeMessage> pipe, object target) : base(pipe, new PipeCallback(target, pipe))
    {
    }

    public class PipeCallback : IPipeCallback<PipeMessage>
    {
      private readonly TaskCompletionSource<object?> _failedOnce = new();
      public Task FailedOnce => _failedOnce.Task;

      private readonly IDictionary<Guid, TaskCompletionSource<object?>> _responses = new Dictionary<Guid, TaskCompletionSource<object?>>();
      private readonly SemaphoreSlim _semaphore = new(1, 1);

      private readonly IPipeClient<PipeMessage> _pipe;

      private readonly object _target;
      private readonly Type _type;
      private readonly IDictionary<string, object> _reflectionCache = new Dictionary<string, object>();

      internal PipeCallback(object target, IPipeClient<PipeMessage> pipe)
      {
        _target = target;
        _pipe = pipe;
        _type = target.GetType();
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

      public void OnMessageReceived(PipeMessage? message)
      {
        Debug.WriteLine(message);
        if (message == null)
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
            property.SetValue(_target, message.Parameter);
            _pipe.WriteAsync(message.ToResponse<object>()).Wait();
          }
          else
          {
            var value = property.GetValue(_target);
            _pipe.WriteAsync(message.ToResponse(value)).Wait();
          }
          return;
        }

        var method = _type.GetMethod(target);
        if (method is null)
        {
          throw new InvalidOperationException($"Method {target} on type {_type.Name} does not exist.");
        }
        var returned = method.Invoke(_target, message.Parameter as object?[]);
        _pipe.WriteAsync(message.ToResponse(returned)).Wait();
      }

      public Task<object?> GetResponse(PipeMessage message)
      {
        var tcs = new TaskCompletionSource<object?>();
        _semaphore.Wait();
        _responses.Add(message.Id, tcs);
        _semaphore.Release();
        return tcs.Task;
      }

      public void OnMessageSent(PipeMessage? message)
      {
        Debug.WriteLine(message);
      }

      public void AddProperty(string propertyName, PropertyInfo property)
      {
        property ??= _type.GetProperty(propertyName) ?? throw new ArgumentNullException();
        var type = property.PropertyType;
        var setter = property.GetSetMethod()?.CreateDelegate(typeof(Action<>).MakeGenericType(type), _target);
        if (setter != null)
        {
          _reflectionCache.Add("set_" + propertyName, setter);
        }
        var getter = property.GetGetMethod()?.CreateDelegate(typeof(Func<>).MakeGenericType(type), _target);
        if (getter != null)
        {
          _reflectionCache.Add("get_" + propertyName, getter);
        }
      }

      public void AddMethod(string methodName, MethodInfo method)
      {
        method ??= _type.GetMethod(methodName) ?? throw new ArgumentNullException();
        var returnType = method.ReturnType;
        var parameters = method.GetParameters();
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
            var action = method.CreateDelegate(type, _target);
            _reflectionCache.Add(methodName, action);
          }
          else if (type != null)
          {
            var action = method.CreateDelegate(type.MakeGenericType(parameterTypes), _target);
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
            var function = method.CreateDelegate(type.MakeGenericType(parameterTypes), _target);
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
      var cmd = new PipeMessage(callerName, parameters);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }

    public void EventRemote(string callerName)
    {
      var cmd = new PipeMessage(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = Callback.GetResponse(cmd).Result;
    }
  }
}