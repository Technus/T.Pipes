using H.Pipes.Args;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class PipeClient<TCallback>
    : PipeClient<H.Pipes.PipeClient<PipeMessage>, PipeMessage, TCallback>
    where TCallback : IPipeCallback<PipeMessage>
  {
    public PipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public PipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class PipeClient<TPacket, TCallback>
    : PipeClient<H.Pipes.PipeClient<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    public PipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public PipeClient(H.Pipes.PipeClient<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class PipeClient<TPipe, TPacket, TCallback>
    : PipeConnection<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    public override bool IsRunning => Pipe.IsConnected;
    public override string PipeName => Pipe.PipeName;
    public override string ServerName => Pipe.ServerName;

    public PipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.Disconnected += OnDisconnected;
      Pipe.Connected += OnConnected;
    }

    private void OnDisconnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      Callback?.Disconnected(e.Connection.PipeName);
    }

    private void OnConnected(object? sender, ConnectionEventArgs<TPacket> e)
    {
      Callback?.Connected(e.Connection.PipeName);
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Pipe.Disconnected -= OnDisconnected;
      Pipe.Connected -= OnConnected;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      await Pipe.ConnectAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
      await Pipe.DisconnectAsync(cancellationToken);
    }
  }
}
