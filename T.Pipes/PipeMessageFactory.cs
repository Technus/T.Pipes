using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class PipeMessageFactory : IPipeMessageFactory<PipeMessage>
  {
    public PipeMessage Create(string command) => new() { Command = command, Id = Guid.NewGuid()};
    public PipeMessage Create(string command, object? parameter) => new() { Command = command, Id = Guid.NewGuid(), Parameter = parameter };
    public PipeMessage CreateResponse(PipeMessage commandMessage) => new() { Command = commandMessage.Command, Id = commandMessage.Id };
    public PipeMessage CreateResponse(PipeMessage commandMessage, object? parameter) => new() { Command = commandMessage.Command, Id = commandMessage.Id, Parameter = parameter };
  }
}
