using System;

namespace T.Pipes
{
  /// <summary>
  /// Generic Uniquely identifiable message
  /// </summary>
  [Serializable]
  public class PipeMessage
  {
    private PipeMessage(string command, Guid id)
    {
      Command = command;
      Id = id;
    }

    public PipeMessage(string command) : this(command, Guid.NewGuid())
    {
    }

    public PipeMessage(string command, object? parameter = default) : this(command) =>
      Parameter = parameter;

    public PipeMessage ToResponse<TResponse>(TResponse? response = default) =>
       new(Command, Id) { Parameter = response };

    public Guid Id { get; private set; }
    public string Command { get; private set; }
    public object? Parameter { get; private set; }

    public override string ToString() => $"{Id} / {Command} / {Parameter}";
  }
}
