using Pastel;
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
    public DelegatingCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target) 
      => LifetimeCancellation.Register(static x => ((IDisposable)x!).Dispose(), this);

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

    public override void Disconnected(string connection)
    {
      base.Disconnected(connection);
      LifetimeCancellationSource.Cancel();
    }

    public override void OnExceptionOccurred(Exception e)
    {
      base.OnExceptionOccurred(e);
      LifetimeCancellationSource.Cancel();
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
    public DelegatingClientAuto(string pipe, TTarget target) : this(new H.Pipes.PipeClient<PipeMessage>(pipe), target)
    {
    }

    public DelegatingClientAuto(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, new(pipe, target))
    {
    }
  }
}