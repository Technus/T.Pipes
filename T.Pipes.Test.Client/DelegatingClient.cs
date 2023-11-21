using H.Pipes;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class DelegatingClient : DelegatingPipeClient<PipeMessage, PipeMessageFactory, Target>
  {
    public DelegatingClient(string pipeName, Target target) : this(new PipeClient<PipeMessage>(pipeName), target)
    {
    }

    public DelegatingClient(IPipeClient<PipeMessage> pipe, Target target) : base(pipe, new PipeMessageFactory(), target)
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
