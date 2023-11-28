using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public abstract class SpawningPipeClientCallback : IPipeCallback<PipeMessage>
  {
    private readonly int _timeoutMs;
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = new();

    protected SpawningPipeClientCallback(int timeoutMs) => _timeoutMs = timeoutMs;

    public virtual void Connected(string connection) => Clear();

    public virtual void Disconnected(string connection)
    {
      Clear();
      throw new InvalidOperationException("Disconnected occurred in client");
    }

    public virtual void Clear()
    {
      foreach (var client in _mapping.Values)
      {
        client.Dispose();
      }
      _mapping.Clear();
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    public virtual ValueTask DisposeAsync()
    {
      Clear();
      return default;
    }

    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear();
    }

    public async virtual void OnMessageReceived(PipeMessage command)
    {
      var proxy = CreateRequestedObjectProxy(command);

      var failedOnce = proxy.Callback.FailedOnce;

      _ = failedOnce.ContinueWith(async x =>
      {
        _mapping.Remove(proxy.ServerName);
        await proxy.DisposeAsync();
        proxy.Callback.Target.Dispose();
      }, TaskContinuationOptions.OnlyOnRanToCompletion);

      _ = failedOnce.ContinueWith(x =>
      {
        _mapping.Remove(proxy.ServerName);
        proxy.Callback.Target.Dispose();
      }, TaskContinuationOptions.OnlyOnCanceled);

      var startTask = proxy.StartAsync();
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(startTask, Task.Delay(_timeoutMs, cts.Token)) == startTask)
      {
        cts.Cancel();
        _mapping.Add(command.Parameter!.ToString()!, proxy);
        return;
      }
      proxy.Dispose();
      proxy.Callback.Target.Dispose();
      throw new InvalidOperationException($"The {nameof(command)}: {command.Command}, could not be performed, connection timed out.");
    }

    public abstract IPipeDelegatingConnection<PipeMessage> CreateRequestedObjectProxy(PipeMessage command);

    public virtual void OnMessageSent(PipeMessage message) { }
  }
}
