using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class ClientCallback : SpawningPipeClientCallback
  {
    public ClientCallback(H.Pipes.PipeClient<PipeMessage> pipe) : base(pipe, PipeConstants.ConnectionAwaitTimeMs)
    {
    }

    public override IPipeDelegatingConnection<PipeMessage> CreateProxy(PipeMessage command) => command.Command switch
    {
      PipeConstants.Create => new DelegatingClientAuto<Target>(command.Parameter!.ToString()!, new Target()),
      _ => throw new ArgumentException($"Invalid command: {command}".Pastel(ConsoleColor.DarkYellow), nameof(command)),
    };
  }

  /// <summary>
  /// Main client used to control Delegating Client instances
  /// </summary>
  internal class Client : SpawningPipeClient<ClientCallback>
  {
    public Client() : this(new H.Pipes.PipeClient<PipeMessage>(PipeConstants.ServerPipeName))
    {
    }

    private Client(H.Pipes.PipeClient<PipeMessage> pipe) : base(pipe, new(pipe))
    {
    }
  }
}
