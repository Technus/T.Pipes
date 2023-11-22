using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  [PipeUse(typeof(IAbstract))]
  [PipeUse(typeof(IAbstract<short>))]
  internal partial class Callback<TTarget>
    : DelegatingPipeClientCallback<TTarget>
    where TTarget : IAbstract, IAbstract<short>
  {
    public Callback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }
  }

  internal class DelegatingClientAuto<TTarget> : DelegatingPipeClient<TTarget, Callback<TTarget>>
    where TTarget : IAbstract, IAbstract<short>
  {
    public DelegatingClientAuto(string pipe, Callback<TTarget> callback) : base(pipe, callback)
    {
    }

    public DelegatingClientAuto(H.Pipes.PipeClient<PipeMessage> pipe, Callback<TTarget> callback) : base(pipe, callback)
    {
    }
  }
}
