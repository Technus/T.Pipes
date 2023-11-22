using H.Pipes;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<int>))]
  internal partial class Callback
    : DelegatingPipeMessageCallback<IPipeServer<PipeMessage>, DelegatingServerAuto>
  {
    public Callback(IPipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  internal class DelegatingServerAuto 
    : DelegatingPipeServer<PipeMessage, PipeMessageFactory, DelegatingServerAuto, Callback>
  {
    public DelegatingServerAuto(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingServerAuto(IPipeServer<PipeMessage> pipe) : base(pipe, new(pipe))
    {
    }
  }
}
