using H.Pipes.Args;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class PipeServer<TCallback>
    : PipeServer<H.Pipes.PipeServer<PipeMessage>, PipeMessage, TCallback>
    where TCallback : IPipeCallback<PipeMessage>
  {
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public PipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class PipeServer<TPacket, TCallback>
    : PipeServer<H.Pipes.PipeServer<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public PipeServer(H.Pipes.PipeServer<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class PipeServer<TPipe, TPacket, TCallback>
    : PipeConnection<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    public override bool IsRunning => Pipe.IsStarted;
    public override string PipeName => Pipe.PipeName;
    public override string ServerName => Pipe.PipeName;

    public PipeServer(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.ClientDisconnected += OnClientDisconnected;
      Pipe.ClientConnected += OnClientConnected;
    }

    private void OnClientDisconnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      Callback?.Disconnected(e.Connection.PipeName);
    }

    private void OnClientConnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      Callback?.Connected(e.Connection.PipeName);
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Pipe.ClientDisconnected -= OnClientDisconnected;
      Pipe.ClientConnected -= OnClientConnected;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      await Pipe.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
      await Pipe.StopAsync(cancellationToken);
    }
  }
}
