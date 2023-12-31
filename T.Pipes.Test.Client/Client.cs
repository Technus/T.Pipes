﻿using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal sealed class ClientCallback : SpawningPipeCallback
  {
    public ClientCallback() : base(PipeConstants.ResponseTimeMs)
    {
    }

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName) => command switch
    {
      PipeConstants.Create => new DelegatingClientAuto<Target>(pipeName, new Target()),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}", nameof(command)),
    };

    public override void OnMessageReceived(PipeMessage message)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Yellow);
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Yellow);
      base.OnMessageSent(message);
    }

    public override void OnDisconnected(string connection)
    {
      LifetimeCancellationSource.Cancel();
      base.OnDisconnected(connection);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      LifetimeCancellationSource.Cancel();
      base.OnExceptionOccurred(exception);
    }
  }

  /// <summary>
  /// Main client used to control Delegating Client instances
  /// </summary>
  internal sealed class Client : SpawningPipeClient<ClientCallback>
  {
    public Client() : this(new H.Pipes.PipeClient<PipeMessage>(PipeConstants.ServerPipeName, formatter: new Formatter()))
    {
    }

    private Client(H.Pipes.PipeClient<PipeMessage> pipe) : base(pipe, new())
    {
    }
  }
}