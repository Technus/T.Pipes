using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  public abstract partial class SpawningPipeClient<TCallback> : PipeServer<TCallback>
    where TCallback : SpawningPipeClientCallback
  {
    private readonly int _timeoutMs;

    protected SpawningPipeClient(string pipe, int timeoutMs, TCallback callback) 
      : this(new H.Pipes.PipeServer<PipeMessage>(pipe), timeoutMs, callback)
    {
    }

    protected SpawningPipeClient(H.Pipes.PipeServer<PipeMessage> pipe, int timeoutMs, TCallback callback) : base(pipe, callback)
    {
      _timeoutMs = timeoutMs;
    }

    public override async ValueTask DisposeAsync()
    {
      await Pipe.DisposeAsync();
      await Callback.DisposeAsync();
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      var startTask = base.StartAsync(cancellationToken);
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(startTask, Task.Delay(_timeoutMs, cts.Token)) == startTask && startTask.IsCompleted)
      {
        cts.Cancel();
        return;
      }
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the server was not started or connection was impossible");
    }
  }
}
