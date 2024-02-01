using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  [PipeUse(typeof(IAbstract))]
  [PipeUse(typeof(IAbstract<short>))]
  internal sealed partial class DelegatingCallback<TTarget>
    : DelegatingPipeClientCallbackAutoDisposing<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingCallback(TTarget target) : base(target, PipeConstants.ResponseTimeMs)
    { 
    }

#if TRACE
    public override Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.DarkYellow);
      return base.OnMessageReceived(message, cancellationToken);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.DarkYellow);
      base.OnMessageSent(message);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      ("E: " + exception.ToString()).WriteLine(ConsoleColor.DarkYellow);
      base.OnExceptionOccurred(exception);
    }
#endif
  }

  internal sealed class DelegatingClientAuto<TTarget> : DelegatingPipeClient<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingClientAuto(string pipe, TTarget target) : base(pipe, new(target))
    {
    }
  }
}