﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public abstract class DelegatingPipeCallback<TPipe, TPacket, TPacketFactory> : IPipeCallback<TPacket>
    where TPipe : IPipeConnection<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    private readonly TaskCompletionSource<object?> _failedOnce = new();
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _responses = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, Func<object?, object?>> _functions = new();

    protected TPipe Pipe { get; }
    protected TPacketFactory PacketFactory { get; }

    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
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
      else if(_functions.TryGetValue(message.Command, out var function))
      {
        Pipe.WriteAsync(PacketFactory.CreateResponse(message, function.Invoke(message.Parameter)));
      }
    }

    public virtual void OnMessageSent(TPacket? message) { }

    public object? GetResponse(TPacket message)
    {
      var tcs = new TaskCompletionSource<object?>();
      _semaphore.Wait();
      _responses.Add(message.Id, tcs);
      _semaphore.Release();
      return tcs.Task.Result;
    }

    public async Task<object?> GetResponse(TPacket message)
    {
      var tcs = new TaskCompletionSource<object?>();
      await _semaphore.WaitAsync();
      _responses.Add(message.Id, tcs);
      _semaphore.Release();
      return await tcs.Task;
    }

    public async Task<TOut?> RemoteAsync<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      return (TOut?) await GetResponse(cmd);
    }

    public async Task<TOut?> RemoteAsync<TOut, TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      return (TOut?) await GetResponse(cmd);
    }

    public async Task RemoteAsync(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      await GetResponse(cmd);
    }

    public async Task RemoteAsync<TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      await GetResponse(cmd);
    }

    public TOut? Remote<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return (TOut?)GetResponse(cmd).Result;
    }

    public TOut? Remote<TOut, TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      return (TOut?)GetResponse(cmd).Result;
    }

    public void Remote(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse(cmd).Result;
    }

    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse(cmd).Result;
    }

    public void SetFunction(string callerName, Func<object?, object?> function) => _functions[callerName] = function;
    public void RemoveFunction(string callerName) => _functions.Remove(callerName);
  }
}