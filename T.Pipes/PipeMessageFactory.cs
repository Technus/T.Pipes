using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Factory for <see cref="PipeMessage"/>
  /// </summary>
  public class PipeMessageFactory : IPipeMessageFactory<PipeMessage>
  {
    /// <inheritdoc/>
    public PipeMessage Create(string command)
    {
      return new() { Command = command, Id = Guid.NewGuid() };
    }

    /// <inheritdoc/>
    public PipeMessage Create(string command, object? parameter)
    {
      return new() { Command = command, Id = Guid.NewGuid(), Parameter = parameter };
    }

    /// <inheritdoc/>
    public PipeMessage CreateResponse(PipeMessage commandMessage)
    {
      return new() { Command = commandMessage.Command, Id = commandMessage.Id };
    }

    /// <inheritdoc/>
    public PipeMessage CreateResponse(PipeMessage commandMessage, object? parameter)
    {
      return new() { Command = commandMessage.Command, Id = commandMessage.Id, Parameter = parameter };
    }
  }
}
