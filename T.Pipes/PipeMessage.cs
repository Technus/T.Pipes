using System;

namespace T.Pipes
{
  /// <summary>
  /// Generic Uniquely identifiable message
  /// </summary>
  [Serializable]
  public class PipeMessage<T> where T : struct
  {
    private PipeMessage(Guid command, Guid transactionId)
    {
      CommandId = command;
      TransactionId = transactionId;
    }

    public PipeMessage(Guid command) : this(command, Guid.NewGuid())
    {
    }

    public PipeMessage(Guid command, T parameter = default) : this(command) =>
      Parameter = parameter;

    public PipeMessage<TResponse> ToResponse<TResponse>(TResponse response = default) where TResponse : struct =>
       new(CommandId, TransactionId) { Parameter = response };

    public Guid TransactionId { get; private set; }
    public Guid CommandId { get; private set; }
    public T Parameter { get; private set; }

    public override string ToString() => $"{TransactionId} / {CommandId} / {Parameter}";
  }
}
