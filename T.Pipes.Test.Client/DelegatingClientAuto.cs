using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  [PipeUse(typeof(IAbstract))]
  [PipeUse(typeof(IAbstract<short>))]
  internal sealed partial class DelegatingCallback<TTarget>
    : DelegatingPipeClientCallback<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingCallback(TTarget target) : base(target, PipeConstants.ResponseTimeMs)
    { 
    }

    public override void OnMessageReceived(PipeMessage message)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Yellow, ConsoleColor.DarkYellow);
      base.OnMessageReceived(message);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Yellow, ConsoleColor.DarkYellow);
      base.OnMessageSent(message);
    }

    public override void OnDisconnected(string connection)
    {
      LifetimeCancellationSource.Cancel();
      base.OnDisconnected(connection);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      LifetimeCancellationSource.Cancel();
      base.OnExceptionOccurred(exception);
    }

    protected override void DisposeCore(bool disposing,bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      Target.Dispose();
    }
  }

  internal sealed class DelegatingClientAuto<TTarget> : DelegatingPipeClient<TTarget, DelegatingCallback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingClientAuto(string pipe, TTarget target) : this(new H.Pipes.PipeClient<PipeMessage>(pipe, formatter: new Formatter()), target)
    {
    }

    public DelegatingClientAuto(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, new(target))
    {
    }
  }
}