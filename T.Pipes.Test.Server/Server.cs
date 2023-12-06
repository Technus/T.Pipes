using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal sealed class ServerCallback : SpawningPipeServerCallback
  {
    public ServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, PipeConstants.ConnectionAwaitTimeMs)
    {
    }

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string name) => command switch
    {
      PipeConstants.Create => new DelegatingServerAuto(name),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}".Pastel(ConsoleColor.DarkCyan), nameof(command)),
    };

    public Task<DelegatingServerAuto> Create() => ProvideProxyAsyncCore<DelegatingServerAuto>(PipeConstants.Create);

    public override void OnMessageReceived(PipeMessage message)
    {
      Console.WriteLine(("I: " + message.ToString()).Pastel(ConsoleColor.Cyan));
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Cyan));
      base.OnMessageSent(message);
    }
  }

  /// <summary>
  /// Main server used to control Delegating Server Instances
  /// </summary>
  internal sealed class Server : SpawningPipeServer<ServerCallback>
  {
    public Server() : this(new H.Pipes.PipeServer<PipeMessage>(PipeConstants.ServerPipeName))
    {
    }

    private Server(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, new(PipeConstants.ClientExeName), new(pipe))
    {
    }
  }
}