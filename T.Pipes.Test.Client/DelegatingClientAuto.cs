using H.Pipes;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  [PipeUse(typeof(IAbstract))]
  [PipeUse(typeof(IAbstract<short>))]
  internal partial class Callback<TTarget> : DelegatingPipeMessageCallback<IPipeClient<PipeMessage>, TTarget> where TTarget : IAbstract, IAbstract<short>
  {
    public Callback(IPipeClient<PipeMessage> pipe, TTarget? target = default) : base(pipe, target)
    {
    }
  }

  internal class DelegatingClientAuto<TTarget> : DelegatingPipeClient<PipeMessage, PipeMessageFactory, TTarget, Callback<TTarget>> where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingClientAuto(string pipeName, TTarget target) : this(new PipeClient<PipeMessage>(pipeName), target)
    {
    }

    public DelegatingClientAuto(IPipeClient<PipeMessage> pipe, TTarget target) : base(pipe, new(pipe, target))
    {
      Callback.Target.Act += Callback.IAbstract_invoke_Act;
      Callback.Target.Set += Callback.IAbstract_invoke_Set;
      Callback.Target.Get += Callback.IAbstract_invoke_Get;
    }

    public async override ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Callback.Target.Act -= Callback.IAbstract_invoke_Act;
      Callback.Target.Set -= Callback.IAbstract_invoke_Set;
      Callback.Target.Get -= Callback.IAbstract_invoke_Get;
    }
  }
}
