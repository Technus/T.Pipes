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
      Callback.Target.Act += Target_Act;
      Callback.Target.Set += Target_Set;
      Callback.Target.Get += Target_Get;
      SetFunctionRemote(x =>
      {
        var (a, b, c) = ((int, int, int))x!;
        (int ret, int d, int e) ret = default;
        ret.ret = Callback.Target.DoIt(a, b, c, out ret.d, out ret.e);
        return ret;
      }, nameof(IAbstract.DoIt));
    }

    public async override ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Callback.Target.Act -= Target_Act;
      Callback.Target.Set -= Target_Set;
      Callback.Target.Get -= Target_Get;
    }

    private void Target_Get(string obj) => EventRemote(obj, nameof(IAbstract.Get));
    private int Target_Set() => EventRemote<int>(nameof(IAbstract.Set));
    private void Target_Act() => EventRemote(nameof(IAbstract.Act));
  }
}
