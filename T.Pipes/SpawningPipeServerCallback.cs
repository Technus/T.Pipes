using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public abstract class SpawningPipeServerCallback : IPipeCallback<PipeMessage>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    public Dictionary<string, IPipeDelegatingConnection<PipeMessage>> Mapping { get; } = [];

    public Task ConnectedOnce => _connectedOnce.Task;

    public virtual void Clear()
    {
      foreach (var server in Mapping.Values)
      {
        server.Dispose();
      }
      Mapping.Clear();
    }

    public virtual ValueTask DisposeAsync()
    {
      _connectedOnce.TrySetCanceled();
      Clear();
      return default;
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    public virtual void Connected(string connection)
    {
      Mapping.Clear();
      _connectedOnce.TrySetResult(null);
    }

    public virtual void Disconnected(string connection)
    {
      Mapping.Clear();
      _connectedOnce.TrySetCanceled();
    }

    public virtual void OnExceptionOccurred(Exception e)
    {
      Mapping.Clear();
      _connectedOnce.TrySetException(e);
    }

    public virtual void OnMessageSent(PipeMessage message) { }

    public virtual void OnMessageReceived(PipeMessage message) { }
  }
}
