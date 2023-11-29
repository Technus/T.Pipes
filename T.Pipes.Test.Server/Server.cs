using System.Diagnostics;
using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class ServerCallback : SpawningPipeServerCallback
  {
    public ServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, PipeConstants.ConnectionAwaitTimeMs)
    {
    }

    public DelegatingServerAuto Create() => RequestProxyAsync<DelegatingServerAuto>(PipeConstants.Create, new(Guid.NewGuid().ToString())).Result;

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
  internal class Server : SpawningPipeServer<ServerCallback>
  {
    public Server() : this(new H.Pipes.PipeServer<PipeMessage>(PipeConstants.ServerPipeName))
    {
    }

    private Server(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, PipeConstants.ClientExeName, new(pipe))
    {
    }
  }
}
