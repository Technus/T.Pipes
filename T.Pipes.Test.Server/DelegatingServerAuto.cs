using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<int>))]
  internal partial class Callback : DelegatingPipeServerCallback<Callback>
  {
    public Callback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  internal class DelegatingServerAuto : DelegatingPipeServer<Callback>
  {
    public DelegatingServerAuto(string pipe, DelegatingPipeServerCallback<Callback> callback) : base(pipe, callback)
    {
    }

    public DelegatingServerAuto(H.Pipes.PipeServer<PipeMessage> pipe, DelegatingPipeServerCallback<Callback> callback) : base(pipe, callback)
    {
    }
  }
}
