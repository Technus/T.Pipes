using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  [PipeUse(typeof(IAbstract))]
  [PipeUse(typeof(IAbstract<short>))]
  internal partial class DelegatingCallback<TTarget>
    : DelegatingPipeClientCallback<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }

    public override void OnMessageReceived(PipeMessage message)
    {
      Console.WriteLine(("I: " + message.ToString()).Pastel(ConsoleColor.Yellow).PastelBg(ConsoleColor.DarkYellow));
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Yellow).PastelBg(ConsoleColor.DarkYellow));
      base.OnMessageSent(message);
    }
  }

  internal class DelegatingClientAuto<TTarget> : DelegatingPipeClient<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingClientAuto(string pipe, TTarget target) : this(new H.Pipes.PipeClient<PipeMessage>(pipe), target)
    {
    }

    public DelegatingClientAuto(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, new(pipe,target))
    {
    }
  }
}
