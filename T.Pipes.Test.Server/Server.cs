using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal sealed class ServerCallback : SpawningPipeCallback
  {
    public ServerCallback() : base(PipeConstants.ResponseTimeMs)
    {
    }

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName) => command switch
    {
      PipeConstants.Create => new DelegatingServerAuto(pipeName),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}", nameof(command)),
    };

    public Task<DelegatingServerAuto> Create() => CreateProxyAsync<DelegatingServerAuto>(PipeConstants.Create);

    public override Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Cyan);
      return base.OnMessageReceived(message, cancellationToken);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Cyan);
      base.OnMessageSent(message);
    }
  }

  /// <summary>
  /// Main server used to control Delegating Server Instances
  /// </summary>
  internal sealed class Server : SpawningPipeServer<ServerCallback>
  {
    public Server() : this(new H.Pipes.PipeServer<PipeMessage>(PipeConstants.ServerPipeName, formatter: new Formatter()))
    {
    }

    private Server(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, new())
    {
    }
  }
}