using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeDelegationTests
  {
    [Fact]
    public async Task Starts()
    {
    }
  }

  [PipeServe(typeof(IAbstract))]
  internal sealed partial class ServerCallback : DelegatingPipeServerCallback<ServerCallback>
  {
    public ServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  internal sealed class Server : DelegatingPipeServer<ServerCallback>
  {
    public Server(string pipe, ServerCallback callback) : base(pipe, callback)
    {
    }

    public Server(H.Pipes.PipeServer<PipeMessage> pipe, ServerCallback callback) : base(pipe, callback)
    {
    }
  }

  [PipeUse(typeof(IAbstract))]
  internal sealed partial class ClientCallback<TTarget> : DelegatingPipeClientCallback<TTarget, ClientCallback<TTarget>>
    where TTarget : IAbstract
  {
    public ClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }
  }

  internal sealed class Client<TTarget> : DelegatingPipeClient<TTarget, ClientCallback<TTarget>>
    where TTarget : IAbstract
  {
    public Client(string pipe, ClientCallback<TTarget> callback) : base(pipe, callback)
    {
    }

    public Client(H.Pipes.PipeClient<PipeMessage> pipe, ClientCallback<TTarget> callback) : base(pipe, callback)
    {
    }
  }
}
