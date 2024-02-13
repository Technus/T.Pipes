namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeConnectionTests
  {
    [Fact]
    public async Task PipeClientStartAndConnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
        try 
        { 
          await client.DisposeAsync(); 
        } 
        catch 
        { 
          // No action required
        }
      }
    }

    [Fact]
    public async Task PipeServerDispose()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
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
        try
        {
          await server.DisposeAsync();
        }
        catch
        {
          // No action required
        }
      }
    }

    [Fact]
    public async Task PipeClientReconnect()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      await server.StartAsync();

      var task = client.StartAsServiceAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(2));

      await server.StopAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(2));

      await server.StartAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(2));

      await server.WriteAsync(new() { Command = "13" });

      await Task.Delay(100);

      await clientCallback.Received(1).OnMessageReceived(Arg.Any<PipeMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PipeClientDisconnectedNoReceive()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      await server.StartAsync();

      await server.WriteAsync(new() { Command = "13" });

      _ = client.StartAsServiceAsync();

      await Task.Delay(100);

      await clientCallback.Received(0).OnMessageReceived(Arg.Any<PipeMessage>(), Arg.Any<CancellationToken>());
    }
  }
}