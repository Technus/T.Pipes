using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeServerCallback<TPacket, TPacketFactory> : IPipeCallback<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new TaskCompletionSource<object?>();
    private readonly TaskCompletionSource<object?> _failedOnce = new TaskCompletionSource<object?>();
    public Task ConnectedOnce => _connectedOnce.Task;
    public Task FailedOnce => _failedOnce.Task;

    private readonly IDictionary<Guid, TaskCompletionSource<object?>> _responses = new Dictionary<Guid, TaskCompletionSource<object?>>();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly IDictionary<string, Func<object?, object?>> _functions = new Dictionary<string, Func<object?, object?>>();

    private readonly IPipeServer<TPacket> _pipe;
    private readonly TPacketFactory _packetFactory;

    internal PipeCallback(IPipeServer<TPacket> pipe, TPacketFactory packetFactory)
    {
      _pipe = pipe;
      _packetFactory = packetFactory;
    }

    public async ValueTask DisposeAsync()
    {
      _functions.Clear();
      await _semaphore.WaitAsync();
      foreach (var item in _responses)
      {
        item.Value.SetCanceled();
      }
      _responses.Clear();
      _semaphore.Dispose();
      _connectedOnce.TrySetCanceled();
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

    public void OnMessageReceived(TPacket? message)
    {
      Debug.WriteLine(message);

      if (message is null)
      {
        return;
      }

      if (_functions.TryGetValue(message.Command, out var function))
      {
        _pipe.WriteAsync(_packetFactory.CreateResponse(message, function.Invoke(message.Parameter))).Wait();
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

    public void OnMessageSent(TPacket? message) => Debug.WriteLine(message);

    public Task<object?> GetResponse(TPacket message)
    {
      var tcs = new TaskCompletionSource<object?>();
      _semaphore.Wait();
      _responses.Add(message.Id, tcs);
      _semaphore.Release();
      return tcs.Task;
    }

    public void AddFunction(string callerName, Func<object?, object?> function) => _functions[callerName] = function;
    public void RemoveFunction(string callerName) => _functions.Remove(callerName);
  }
}
