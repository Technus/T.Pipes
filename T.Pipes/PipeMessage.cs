using System;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Generic Uniquely identifiable message
  /// </summary>
  [Serializable]
  public struct PipeMessage : IPipeMessage
  {
    public Guid Id { get; set; }
    public string Command { get; set; }
    public object? Parameter { get; set; }

    public override string ToString()
    {
      return $"{Id} / {Command} / {Parameter}";
    }
  }
}
