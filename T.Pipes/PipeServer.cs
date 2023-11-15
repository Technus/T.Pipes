using System.Threading;
using System.Threading.Tasks;
using H.Pipes;
using H.Pipes.Args;

namespace T.Pipes
{
  public class PipeServer<TPacket, TCallback> : PipeConnection<IPipeServer<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    public override bool IsRunning => Pipe.IsStarted;
    public override string PipeName => Pipe.PipeName;
    public override string ServerName => Pipe.PipeName;

    public PipeServer(string pipeName, TCallback callback) : this(new PipeServer<TPacket>(pipeName), callback)
    {
    }

    public PipeServer(IPipeServer<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.ClientDisconnected += OnClientDisconnected;
      Pipe.ClientConnected += OnClientConnected;
    }

    private void OnClientDisconnected(object? sender, ConnectionEventArgs<TPacket> e) => Callback?.Disconnected(e.Connection.PipeName);
    private void OnClientConnected(object? sender, ConnectionEventArgs<TPacket> e) => Callback?.Connected(e.Connection.PipeName);

    protected override async ValueTask DisposeManagedAsync()
    {
      await base.DisposeManagedAsync();
      Pipe.ClientDisconnected -= OnClientDisconnected;
      Pipe.ClientConnected -= OnClientConnected;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default) => await Pipe.StartAsync(cancellationToken);
    public override async Task StopAsync(CancellationToken cancellationToken = default) => await Pipe.StopAsync(cancellationToken);
  }
}
