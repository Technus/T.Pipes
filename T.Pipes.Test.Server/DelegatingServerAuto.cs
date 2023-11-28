using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<short>))]
  internal partial class DelegatingCallback : DelegatingPipeServerCallback<DelegatingCallback>
  {
    public DelegatingCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }

    public override void OnMessageReceived(PipeMessage message)
    {
      Console.WriteLine(("I: " + message.ToString()).Pastel(ConsoleColor.Cyan).PastelBg(ConsoleColor.DarkCyan));
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Cyan).PastelBg(ConsoleColor.DarkCyan));
      base.OnMessageSent(message);
    }
  }

  internal class DelegatingServerAuto : DelegatingPipeServer<DelegatingCallback>
  {
    public DelegatingServerAuto(string pipe) : this(new H.Pipes.PipeServer<PipeMessage>(pipe))
    {
    }

    public DelegatingServerAuto(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, new(pipe))
    {
    }
  }
}
