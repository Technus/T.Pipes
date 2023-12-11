﻿using H.Formatters;
using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal sealed class ClientCallback : SpawningPipeClientCallback
  {
    public ClientCallback(H.Pipes.PipeClient<PipeMessage> pipe) : base(pipe, PipeConstants.ConnectionAwaitTimeMs)
    {
    }

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName) => command switch
    {
      PipeConstants.Create => new DelegatingClientAuto<Target>(pipeName, new Target()),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}".Pastel(ConsoleColor.DarkYellow), nameof(command)),
    };

    public override void OnMessageReceived(PipeMessage message)
    {
      Console.WriteLine(("I: " + message.ToString()).Pastel(ConsoleColor.Yellow));
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Yellow));
      base.OnMessageSent(message);
    }

    public override void OnDisconnected(string connection)
    {
      base.OnDisconnected(connection);
      LifetimeCancellationSource.Cancel();
    }

    public override void OnExceptionOccurred(Exception e)
    {
      base.OnExceptionOccurred(e);
      LifetimeCancellationSource.Cancel();
    }
  }

  /// <summary>
  /// Main client used to control Delegating Client instances
  /// </summary>
  internal sealed class Client : SpawningPipeClient<ClientCallback>
  {
    public Client() : this(new H.Pipes.PipeClient<PipeMessage>(PipeConstants.ServerPipeName, formatter: new CerasFormatter()))
    {
    }

    private Client(H.Pipes.PipeClient<PipeMessage> pipe) : base(pipe, new(pipe))
    {
    }
  }
}