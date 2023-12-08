namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeConnectionTests
  {
    [Fact]
    public async Task PipeClientStartAndConnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      await server.StartAsync();

      var task = client.StartAndConnectAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PipeServerStartAndConnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      await client.StartAsync();

      var task = server.StartAndConnectAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PipeClientDisconnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      serverCallback.Received(0).OnConnected(Arg.Any<string>());
      clientCallback.Received(0).OnConnected(Arg.Any<string>());
      serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

      await client.StartAsync();
      await server.StartAndConnectAsync();
      await Task.Delay(100);

      serverCallback.Received(1).OnConnected(Arg.Any<string>());
      clientCallback.Received(1).OnConnected(Arg.Any<string>());
      serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

      await client.StopAsync();
      await Task.Delay(100);

      serverCallback.Received(1).OnConnected(Arg.Any<string>());
      clientCallback.Received(1).OnConnected(Arg.Any<string>());
      serverCallback.Received(1).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(1).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
    }

    [Fact]
    public async Task PipeServerDisconnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      serverCallback.Received(0).OnConnected(Arg.Any<string>());
      clientCallback.Received(0).OnConnected(Arg.Any<string>());
      serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

      await client.StartAsync();
      await server.StartAndConnectAsync();
      await Task.Delay(100);

      serverCallback.Received(1).OnConnected(Arg.Any<string>());
      clientCallback.Received(1).OnConnected(Arg.Any<string>());
      serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

      await server.StopAsync();
      await Task.Delay(100);

      serverCallback.Received(1).OnConnected(Arg.Any<string>());
      clientCallback.Received(1).OnConnected(Arg.Any<string>());
      serverCallback.Received(1).OnDisconnected(Arg.Any<string>());
      clientCallback.Received(1).OnDisconnected(Arg.Any<string>());
      serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
    }

    [Fact]
    public async Task PipeClientDispose()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      try
      {
        serverCallback.Received(0).OnConnected(Arg.Any<string>());
        clientCallback.Received(0).OnConnected(Arg.Any<string>());
        serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

        await client.StartAsync();
        await server.StartAndConnectAsync();
        await Task.Delay(100);

        serverCallback.Received(1).OnConnected(Arg.Any<string>());
        clientCallback.Received(1).OnConnected(Arg.Any<string>());
        serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

        await client.DisposeAsync();
        await Task.Delay(100);

        serverCallback.Received(1).OnConnected(Arg.Any<string>());
        clientCallback.Received(1).OnConnected(Arg.Any<string>());
        serverCallback.Received(1).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(1).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      }
      finally
      {
        try { await client.DisposeAsync(); } catch { }
      }
    }

    [Fact]
    public async Task PipeServerDispose()
    {
      var pipeName = Guid.NewGuid().ToString();
      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      try
      {
        serverCallback.Received(0).OnConnected(Arg.Any<string>());
        clientCallback.Received(0).OnConnected(Arg.Any<string>());
        serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

        await client.StartAsync();
        await server.StartAndConnectAsync();
        await Task.Delay(100);

        serverCallback.Received(1).OnConnected(Arg.Any<string>());
        clientCallback.Received(1).OnConnected(Arg.Any<string>());
        serverCallback.Received(0).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(0).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());

        await server.DisposeAsync();
        await Task.Delay(100);

        serverCallback.Received(1).OnConnected(Arg.Any<string>());
        clientCallback.Received(1).OnConnected(Arg.Any<string>());
        serverCallback.Received(1).OnDisconnected(Arg.Any<string>());
        clientCallback.Received(1).OnDisconnected(Arg.Any<string>());
        serverCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
        clientCallback.Received(0).OnExceptionOccurred(Arg.Any<Exception>());
      }
      finally
      {
        try { await server.DisposeAsync(); } catch { }
      }
    }
  }
}