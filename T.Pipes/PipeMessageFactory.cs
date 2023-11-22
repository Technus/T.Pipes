using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class PipeMessageFactory : IPipeMessageFactory<PipeMessage>
  {
    public PipeMessage Create(string command)
    {
      return new() { Command = command, Id = Guid.NewGuid() };
    }

    public PipeMessage Create(string command, object? parameter)
    {
      return new() { Command = command, Id = Guid.NewGuid(), Parameter = parameter };
    }

    public PipeMessage CreateResponse(PipeMessage commandMessage)
    {
      return new() { Command = commandMessage.Command, Id = commandMessage.Id };
    }

    public PipeMessage CreateResponse(PipeMessage commandMessage, object? parameter)
    {
      return new() { Command = commandMessage.Command, Id = commandMessage.Id, Parameter = parameter };
    }
  }
}
