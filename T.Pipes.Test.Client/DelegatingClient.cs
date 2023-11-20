using H.Pipes;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class DelegatingClient<TTarget> : DelegatingPipeClient<TTarget, PipeMessage, PipeMessageFactory> where TTarget : IAbstract
  {
    public DelegatingClient(string pipeName, TTarget target) : this(new PipeClient<PipeMessage>(pipeName), target)
    {
    }

    public DelegatingClient(IPipeClient<PipeMessage> pipe, TTarget target) : base(new PipeMessageFactory(), pipe, target)
    {
      Target.Act += Target_Act;
      Target.Set += Target_Set;
      Target.Get += Target_Get;
      AddFunctionRemote(x =>
      {
        (int a, int b, int c) = ((int a, int b, int c))x!;
        (int ret, int d, int e) ret = default;
        ret.ret = Target.DoIt(a, b, c, out ret.d, out ret.e);
        return ret;
      }, nameof(IAbstract.DoIt));
    }

    public async override ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Target.Act -= Target_Act;
      Target.Set -= Target_Set;
      Target.Get -= Target_Get;
    }

    private void Target_Get(string obj) => EventRemote(obj, nameof(IAbstract.Get));
    private int Target_Set() => EventRemote<int>(nameof(IAbstract.Set));
    private void Target_Act() => EventRemote(nameof(IAbstract.Act));
  }
}
