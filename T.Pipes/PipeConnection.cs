using System.Threading;
using System.Threading.Tasks;
using H.Pipes.Args;
using Idefix.BaseClasses.Patterns;

namespace T.Pipes
{
  public abstract class PipeConnection<TPipe, TPacket, TCallback> : IPipeConnection<TPacket>
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

    private void OnMessageReceived(object? sender, ConnectionMessageEventArgs<TPacket?> e) => Callback.OnMessageReceived(e.Message);
    private void OnExceptionOccurred(object? sender, ExceptionEventArgs e) => Callback.OnExceptionOccurred(e.Exception);

    protected override async ValueTask DisposeManagedAsync()
    {
      await base.DisposeManagedAsync();
      await Pipe.DisposeAsync();
      Pipe.MessageReceived -= OnMessageReceived;
      Pipe.ExceptionOccurred -= OnExceptionOccurred;
    }

    public async Task WriteAsync(TPacket value, CancellationToken cancellationToken = default)
    {
      await Pipe.WriteAsync(value, cancellationToken);
      Callback.OnMessageSent(value);
    }

    public abstract Task StartAsync(CancellationToken cancellationToken = default);
    public abstract Task StopAsync(CancellationToken cancellationToken = default);
  }
}
