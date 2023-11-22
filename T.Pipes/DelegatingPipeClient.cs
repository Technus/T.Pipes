using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class DelegatingPipeClient<TTarget, TCallback>
      : DelegatingPipeClient<H.Pipes.PipeClient<PipeMessage>, TTarget, TCallback>
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeClient(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    public DelegatingPipeClient(H.Pipes.PipeClient<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeClient<TPipe, TTarget, TCallback>
    : DelegatingPipeClient<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TPipe : H.Pipes.IPipeClient<PipeMessage>
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
  {
    public DelegatingPipeClient(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  public class DelegatingPipeClient<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeClient<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeClient<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget>
  {
    public TTarget Target => Callback.Target;

    public DelegatingPipeClient(TPipe pipe, TCallback callback)
      : base(pipe, callback)
    {
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await Callback.DisposeAsync();
    }
  }
}