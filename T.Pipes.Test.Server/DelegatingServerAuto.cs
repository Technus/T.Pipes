using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<short>))]
  internal sealed partial class DelegatingCallback : DelegatingPipeServerCallback<DelegatingCallback>
  {
    public DelegatingCallback() : base(PipeConstants.ResponseTimeMs)
    {
    }

    public override void OnMessageReceived(PipeMessage message)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Cyan, ConsoleColor.DarkCyan);
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Cyan, ConsoleColor.DarkCyan);
      base.OnMessageSent(message);
    }
  }

  internal sealed class DelegatingServerAuto : DelegatingPipeServer<DelegatingCallback>
  {
    public DelegatingServerAuto(string pipe) : this(new H.Pipes.PipeServer<PipeMessage>(pipe, formatter: new Formatter()))
    {
    }

    public DelegatingServerAuto(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, new())
    {
    }
  }
}