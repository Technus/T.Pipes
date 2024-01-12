namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeDelegationTests
  {
    [Fact]
    public async Task CorrectClientPipe()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);

      dut.Connection.Should().Be(pipe);
    }

    [Fact]
    public async Task CorrectServerPipe()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      dut.Connection.Should().Be(pipe);
    }

    [Fact]
    public async Task CorrectClientTarget()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);

      dut.Target.Should().Be(target);
      ((IPipeDelegatingCallback<PipeMessage>)dut).Target.Should().Be(target);
    }

    [Fact]
    public async Task CorrectServerTarget()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      dut.Target.Should().Be(dut);
      ((IPipeDelegatingCallback<PipeMessage>)dut).Target.Should().Be(dut);
    }

    [Fact]
    public async Task UnknownServerMessage()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new() { })).Should().Throw<Exception>();
    }

    [Fact]
    public async Task UnknownClientMessage()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new(){ })).Should().Throw<Exception>();
    }

    [Fact]
    public async Task UnknownServerCommand()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Command })).Should().Throw<Exception>();
    }

    [Fact]
    public async Task UnknownClientCommand()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Command })).Should().Throw<Exception>();
    }

    [Fact]
    public async Task UnknownServerResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Response })).Should().NotThrow();
    }

    [Fact]
    public async Task UnknownClientResponse()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Response })).Should().NotThrow();
    }

    [Fact]
    public async Task ClearCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.ClearResponses();

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().NotBeOfType<NoResponseException>();
    }

    [Fact]
    public async Task ConnectedCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.OnConnected("Egg");

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task DisconnectedCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.OnDisconnected("Egg");

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task ExceptionCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      var failure = new Exception("Egg");
      dut.OnExceptionOccurred(failure);

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().BeOfType<Exception>().Which.Message.Should().Be("Egg");
    }

    [Fact]
    public async Task DisposeCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.Dispose();

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().BeOfType<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposeAsyncCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      await dut.DisposeAsync();

      (await task.Should().ThrowAsync<NoResponseException>()).Which.InnerException.Should().BeOfType<ObjectDisposedException>();
    }

    [Fact]
    public async Task TimeoutCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);
      dut.ResponseTimeoutMs = 200;

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      (await task.Should().ThrowAsync<NoResponseException>("In case it was supposedly sent NoResponseException should be thrown")).Which.InnerException.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public async Task ShortTimeoutCallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      await using var pipe = new PipeServer<ServerCallback>(name, dut);
      dut.ResponseTimeoutMs = 0;

      var task = Task.Run(dut.AsIAbstract.Action);

      (await task.Should().ThrowWithinAsync<NoResponseException>(TimeSpan.FromMilliseconds(100))).Which.InnerException.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public async Task CallbackTimeoutCommandWithoutAutoReconnect()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      dut.PacketFactory = new PipeMessageFactory(0);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);
      pipe.Pipe.AutoReconnect = false;
      dut.ResponseTimeoutMs = 200;

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Command, Command = "Action_IAbstract", Id = 1 }))
        .Should().Throw<AggregateException>().Which.InnerExceptions.Count.Should().Be(2, "because the command and the fallback response should fail");
    }

    [Fact]
    public async Task CallbackTimeoutCommandWithAutoReconnect()
    {
      var name = Guid.NewGuid().ToString();
      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      dut.PacketFactory = new PipeMessageFactory(0);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);
      pipe.Pipe.AutoReconnect = true;
      dut.ResponseTimeoutMs = 200;

      dut.Invoking(x => x.OnMessageReceived(new() { PacketType = PacketType.Command, Command = "Action_IAbstract", Id = 1 }))
        .Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public async Task CallbackCommand()
    {
      var name = Guid.NewGuid().ToString();
      await using var callback = new ServerCallback();
      await using var dump = new PipeServer<ServerCallback>(name, callback);
      await dump.StartAsync();

      var target = Substitute.For<IAbstract>();
      await using var dut = new ClientCallback<IAbstract>(target);
      await using var pipe = new PipeClient<ClientCallback<IAbstract>>(name, dut);
      await pipe.StartAsync();
      dut.ResponseTimeoutMs = 200;
      
      dut.OnMessageReceived(new() { PacketType = PacketType.Command, Command = "Action_IAbstract", Id = 1 });
      
      target.Received(1).Action();
    }

    [Fact]
    public async Task CallbackResponse()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      dut.PacketFactory = new PipeMessageFactory(0);
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      var task = Task.Run(dut.AsIAbstract.Action);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.OnMessageReceived(new() { PacketType = PacketType.Response, Command = "Action_IAbstract", Id = 1 });/// Id should be 1 here when using <see cref="PipeMessageFactory"/>

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task CallbackResponseGet()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      dut.PacketFactory = new PipeMessageFactory(0);
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      int result = default;

      var task = Task.Run(() => result = dut.AsIAbstract.GetInt());

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.OnMessageReceived(new() { PacketType = PacketType.Response, Command = "Should be ignored...", Id = 1 , Parameter = 21});/// Id should be 1 here when using <see cref="PipeMessageFactory"/>

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(100));
      (await task).Should().Be(21);
    }

    [Fact]
    public async Task CallbackResponseSet()
    {
      var name = Guid.NewGuid().ToString();
      await using var dut = new ServerCallback();
      dut.PacketFactory = new PipeMessageFactory(0);
      await using var pipe = new PipeServer<ServerCallback>(name, dut);

      int result = default;

      var task = Task.Run(() => dut.AsIAbstract.SetInt(result));

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromMilliseconds(100));

      dut.OnMessageReceived(new() { PacketType = PacketType.Response, Command = "Should be ignored...", Id = 1});/// Id should be 1 here when using <see cref="PipeMessageFactory"/>

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(100));
    }
  }

  [ExcludeFromCodeCoverage]
  [PipeServe(typeof(IAbstract))]
  internal sealed partial class ServerCallback : DelegatingPipeServerCallback<ServerCallback>
  {
    public ServerCallback() : base() 
      => PacketFactory = new();
  }

  [ExcludeFromCodeCoverage]
  internal sealed class Server : DelegatingPipeServer<ServerCallback>
  {
    public Server(string pipe, ServerCallback callback) : base(pipe, callback)
    {
    }

    public Server(H.Pipes.PipeServer<PipeMessage> pipe, ServerCallback callback) : base(pipe, callback)
    {
    }
  }

  [ExcludeFromCodeCoverage]
  [PipeUse(typeof(IAbstract))]
  internal sealed partial class ClientCallback<TTarget> : DelegatingPipeClientCallback<TTarget, ClientCallback<TTarget>>
    where TTarget : IAbstract
  {
    public ClientCallback(TTarget target) : base(target) 
      => PacketFactory = new();
  }

  [ExcludeFromCodeCoverage]
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
