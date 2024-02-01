using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal sealed class ClientCallback : SpawningPipeCallbackSelfDisposing
  {
    public ClientCallback() : base(PipeConstants.ResponseTimeMs)
    {
    }

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName) => command switch
    {
      PipeConstants.Create => new DelegatingClientAuto<Target>(pipeName, new Target()),
      PipeConstants.CreateInvalid => throw new InvalidOperationException("Creation of invalid object (Simulates some error occurring)"),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}", nameof(command)),
    };

#if TRACE
    public override Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Yellow);
      return base.OnMessageReceived(message, cancellationToken);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Yellow);
      base.OnMessageSent(message);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      ("E: " + exception.ToString()).WriteLine(ConsoleColor.Yellow);
      base.OnExceptionOccurred(exception);
    }
#endif
  }

  /// <summary>
  /// Main client used to control Delegating Client instances
  /// </summary>
  internal sealed class Client : SpawningPipeClient<ClientCallback>
  {
    public Client(string name)
      : base(name, new())
    {
    }
  }
}