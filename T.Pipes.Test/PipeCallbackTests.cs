namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeCallbackTests
  {
    [Fact]
    public async Task PipeClientProperCallback()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();

      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      client.Callback.Should().Be(clientCallback);
      (client as IPipeConnection<PipeMessage>).Callback.Should().Be(clientCallback);
    }

    [Fact]
    public async Task PipeServerProperCallback()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();

      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      server.Callback.Should().Be(serverCallback);
      (server as IPipeConnection<PipeMessage>).Callback.Should().Be(serverCallback);
    }
  }
}
