namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeTalkingTests
  {
    [Fact]
    public async Task StringSendTest()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<string>>();
      await using var server = new PipeServer<string, IPipeCallback<string>>(pipeName, serverCallback);

      await using var clientCallback = Substitute.For<IPipeCallback<string>>();
      await using var client = new PipeClient<string, IPipeCallback<string>>(pipeName, clientCallback);

      await server.StartAsync();
      await client.StartAndConnectAsync();

      serverCallback.Received(0).OnMessageSent(Arg.Any<string>());
      clientCallback.Received(0).OnMessageSent(Arg.Any<string>());

      await client.WriteAsync("clientSent");

      await Task.Delay(100);

      serverCallback.Received(0).OnMessageSent(Arg.Any<string>());
      clientCallback.Received(1).OnMessageSent("clientSent");
      clientCallback.ClearReceivedCalls();

      await server.WriteAsync("serverSent");

      serverCallback.Received(1).OnMessageSent("serverSent");
      serverCallback.ClearReceivedCalls();
      clientCallback.Received(0).OnMessageSent(Arg.Any<string>());

      await Task.Delay(100);
    }

    [Fact]
    public async Task StringReceiveTest()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<string>>();
      await using var server = new PipeServer<string, IPipeCallback<string>>(pipeName, serverCallback);

      await using var clientCallback = Substitute.For<IPipeCallback<string>>();
      await using var client = new PipeClient<string, IPipeCallback<string>>(pipeName, clientCallback);

      await server.StartAsync();
      await client.StartAndConnectAsync();

      await serverCallback.Received(0).OnMessageReceived(Arg.Any<string>());
      await clientCallback.Received(0).OnMessageReceived(Arg.Any<string>());

      await client.WriteAsync("clientSent");

      await Task.Delay(100);

      await serverCallback.Received(1).OnMessageReceived("clientSent");
      serverCallback.ClearReceivedCalls();
      await clientCallback.Received(0).OnMessageReceived(Arg.Any<string>());

      await server.WriteAsync("serverSent");

      await Task.Delay(100);

      await serverCallback.Received(0).OnMessageReceived(Arg.Any<string>());
      await clientCallback.Received(1).OnMessageReceived("serverSent");
      clientCallback.ClearReceivedCalls();
    }
  }
}
