using H.Pipes.Args;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public abstract class PipeConnection<TPipe, TPacket, TCallback>
    : IPipeConnection<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    public TPipe Pipe { get; }
    public TCallback Callback { get; }
    public abstract bool IsRunning { get; }
    public abstract string PipeName { get; }
    public abstract string ServerName { get; }

    public PipeConnection(TPipe pipe, TCallback callback)
    {
      Pipe = pipe;
      Callback = callback;

      Pipe.ExceptionOccurred += OnExceptionOccurred;
      Pipe.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object? sender, ConnectionMessageEventArgs<TPacket?> e)
    {
      Callback.OnMessageReceived(e.Message);
    }

    private void OnExceptionOccurred(object? sender, ExceptionEventArgs e)
    {
      Callback.OnExceptionOccurred(e.Exception);
    }

    public virtual async ValueTask DisposeAsync()
    {
      await Pipe.DisposeAsync();
      Pipe.MessageReceived -= OnMessageReceived;
      Pipe.ExceptionOccurred -= OnExceptionOccurred;
    }

    public void Dispose()
    {
      DisposeAsync().AsTask().Wait();
    }

    public async Task WriteAsync(TPacket value, CancellationToken cancellationToken = default)
    {
      Callback.OnMessageSent(value);
      await Pipe.WriteAsync(value, cancellationToken);
    }

    public abstract Task StartAsync(CancellationToken cancellationToken = default);
    public abstract Task StopAsync(CancellationToken cancellationToken = default);
  }
}
