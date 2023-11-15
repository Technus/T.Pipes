using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using H.Pipes.Args;

namespace T.Pipes
{
  public class PipeClient<TPacket, TCallback> : PipeConnection<IPipeClient<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    public override bool IsRunning => Pipe.IsConnected;
    public override string PipeName => Pipe.PipeName;
    public override string ServerName => Pipe.ServerName;

    public PipeClient(string pipeName, TCallback callback) : this(new PipeClient<TPacket>(pipeName), callback)
    {
    }

    public PipeClient(IPipeClient<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.Disconnected += OnDisconnected;
      Pipe.Connected += OnConnected;
    }

    private void OnDisconnected(object? sender, ConnectionEventArgs<TPacket> e) => Callback?.Disconnected(e.Connection.PipeName);
    private void OnConnected(object? sender, ConnectionEventArgs<TPacket> e) => Callback?.Connected(e.Connection.PipeName);

    protected override async ValueTask DisposeManagedAsync()
    {
      await base.DisposeManagedAsync();
      Pipe.Disconnected -= OnDisconnected;
      Pipe.Connected -= OnConnected;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default) => await Pipe.ConnectAsync(cancellationToken);
    public override async Task StopAsync(CancellationToken cancellationToken = default) => await Pipe.DisconnectAsync(cancellationToken);
  }
}
