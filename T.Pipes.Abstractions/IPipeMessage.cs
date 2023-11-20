using System;

namespace T.Pipes.Abstractions
{
  public interface IPipeMessage
  {
    public Guid Id { get; set; }
    public string Command { get; set; }
    public object? Parameter { get; set; }
  }
}
