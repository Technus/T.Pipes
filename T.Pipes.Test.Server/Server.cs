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
