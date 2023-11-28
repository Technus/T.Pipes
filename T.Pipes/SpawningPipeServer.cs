using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public abstract class SpawningPipeServer<TCallback> : PipeServer<TCallback>
    where TCallback : SpawningPipeServerCallback
  {
    private readonly Process _process;

    private readonly int _timeoutMs;

    public SpawningPipeServer(string pipe, string clientExeName, int timeoutMs, TCallback callback) 
      : this(new H.Pipes.PipeServer<PipeMessage>(pipe), clientExeName, timeoutMs, callback)
    {
    }

    public SpawningPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, string clientExeName, int timeoutMs, TCallback callback) : base(pipe, callback)
    {
      _process = new Process { StartInfo = new(clientExeName) };
      _timeoutMs = timeoutMs;
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
      _process.Close();
      _process.Dispose();
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      var startTask = base.StartAsync(cancellationToken);
      await startTask;
      if(startTask.IsCompleted)
      {
        _process.Start();
        var connectedTask = Callback.ConnectedOnce;
        using var cts = new CancellationTokenSource();
        if (await Task.WhenAny(connectedTask, Task.Delay(_timeoutMs, cts.Token)) == connectedTask)
        {
          cts.Cancel();
          return;
        }
        _process.Close();
        await Pipe.StopAsync();
      }
      throw new InvalidOperationException($"Either the client was not started or connection was impossible");
    }

    protected async Task<T> CreateInternal<T>(string command, T implementationServer)
      where T : IPipeDelegatingConnection<PipeMessage>
    {
      var failedOnce = implementationServer.Callback.FailedOnce;

      _ = failedOnce.ContinueWith(x =>
      {
        Callback.Mapping.Remove(implementationServer.ServerName);
        _ = implementationServer.DisposeAsync();
      }, TaskContinuationOptions.OnlyOnRanToCompletion);

      _ = failedOnce.ContinueWith(x =>
        Callback.Mapping.Remove(implementationServer.ServerName), TaskContinuationOptions.OnlyOnCanceled);

      await implementationServer.StartAsync();
      _ = Pipe.WriteAsync(PipeMessageFactory.Instance.Create(command, implementationServer.ServerName));
      var connectedTask = implementationServer.Callback.ConnectedOnce;
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(connectedTask, Task.Delay(_timeoutMs)) == connectedTask)
      {
        cts.Cancel();
        Callback.Mapping.Add(implementationServer.ServerName, implementationServer);
        return implementationServer;
      }
      _ = implementationServer.DisposeAsync();
      throw new InvalidOperationException($"The {nameof(command)}: {command}, could not be performed, connection timed out.");
    }
  }
}
