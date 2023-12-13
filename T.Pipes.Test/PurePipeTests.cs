namespace T.Pipes.Test
{
  using H.Pipes;

  [ExcludeFromCodeCoverage]
  public class PurePipeTests
  {
    [Fact]
    public async Task CallingConnectAsyncDoesNotDuplicateConnectEvents()
    {
      var name = Guid.NewGuid().ToString();
      await using var server = new PipeServer<int>(name);
      await using var client = new PipeClient<int>(name);

      var serverStatus = 0;
      var clientStatus = 0;
      var serverConnections = 0;
      var clientConnections = 0;

      server.MessageReceived += (o, e) => serverStatus = e.Message;
      server.ExceptionOccurred += (o, e) => serverStatus = e.Exception.GetType().GetHashCode();
      server.ClientConnected += (o, e) => Interlocked.Increment(ref serverConnections);
      server.ClientDisconnected += (o, e) => Interlocked.Decrement(ref serverConnections);

      client.MessageReceived += (o, e) => clientStatus = e.Message;
      client.ExceptionOccurred += (o, e) => clientStatus = e.Exception.GetType().GetHashCode();
      client.Connected += (o, e) => Interlocked.Increment(ref clientConnections);
      client.Disconnected += (o, e) => Interlocked.Decrement(ref clientConnections);

      for (int i = 0; i < 1000; i++)
      {
        _ = client.ConnectAsync();//Makes 1000 awaiting tasks in background...
      }

      await server.StartAsync();

      await client.ConnectAsync();

      await Task.Delay(10);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);

      await server.WriteAsync(21);
      await client.WriteAsync(37);

      await Task.Delay(10);

      serverStatus.Should().Be(37);  
      clientStatus.Should().Be(21);
    }

    [Fact]
    public async Task ServerShortRestartDoesNotNeedClientRestartAsync()
    {
      var name = Guid.NewGuid().ToString();
      await using var server = new PipeServer<int>(name);
      await using var client = new PipeClient<int>(name);

      var serverStatus = 0;
      var clientStatus = 0;
      var serverConnections = 0;
      var clientConnections = 0;

      server.MessageReceived += (o, e) => serverStatus = e.Message;
      server.ExceptionOccurred += (o, e) => serverStatus = e.Exception.GetType().GetHashCode();
      server.ClientConnected += (o, e) => Interlocked.Increment(ref serverConnections);
      server.ClientDisconnected += (o, e) => Interlocked.Decrement(ref serverConnections);

      client.MessageReceived += (o, e) => clientStatus = e.Message;
      client.ExceptionOccurred += (o, e) => clientStatus = e.Exception.GetType().GetHashCode();
      client.Connected += (o, e) => Interlocked.Increment(ref clientConnections);
      client.Disconnected += (o, e) => Interlocked.Decrement(ref clientConnections);

      await server.StartAsync();

      await client.ConnectAsync();

      await Task.Delay(10);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);

      await server.WriteAsync(21);
      await client.WriteAsync(37);

      await Task.Delay(10);

      serverStatus.Should().Be(37);
      clientStatus.Should().Be(21);

      await server.StopAsync();

      await Task.Delay(10);

      serverConnections.Should().Be(0);
      //clientConnections.Should().Be(0);

      await server.StartAsync();

      await Task.Delay(4000);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);
    }

    [Fact]
    public async Task ServerLongRestartNeedsClientRestartAsync()
    {
      var name = Guid.NewGuid().ToString();
      await using var server = new PipeServer<int>(name);
      await using var client = new PipeClient<int>(name);

      var serverStatus = 0;
      var clientStatus = 0;
      var serverConnections = 0;
      var clientConnections = 0;

      server.MessageReceived += (o, e) => serverStatus = e.Message;
      server.ExceptionOccurred += (o, e) => serverStatus = e.Exception.GetType().GetHashCode();
      server.ClientConnected += (o, e) => Interlocked.Increment(ref serverConnections);
      server.ClientDisconnected += (o, e) => Interlocked.Decrement(ref serverConnections);

      client.MessageReceived += (o, e) => clientStatus = e.Message;
      client.ExceptionOccurred += (o, e) => clientStatus = e.Exception.GetType().GetHashCode();
      client.Connected += (o, e) => Interlocked.Increment(ref clientConnections);
      client.Disconnected += (o, e) => Interlocked.Decrement(ref clientConnections);

      await server.StartAsync();

      await client.ConnectAsync();

      await Task.Delay(10);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);

      await server.WriteAsync(21);
      await client.WriteAsync(37);

      await Task.Delay(100);

      serverStatus.Should().Be(37);
      clientStatus.Should().Be(21);

      await server.StopAsync();

      await Task.Delay(4000);

      serverConnections.Should().Be(0);
      clientConnections.Should().Be(0);

      await server.StartAsync();
      await client.ConnectAsync();//Sadly after long break it is needed...

      await Task.Delay(4000);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);
    }

    [Fact]
    public async Task ClientRestartNeverNeedsServerRestartAsync()
    {
      var name = Guid.NewGuid().ToString();
      await using var server = new PipeServer<int>(name);
      await using var client = new PipeClient<int>(name);

      var serverStatus = 0;
      var clientStatus = 0;
      var serverConnections = 0;
      var clientConnections = 0;

      server.MessageReceived += (o, e) => serverStatus = e.Message;
      server.ExceptionOccurred += (o, e) => serverStatus = e.Exception.GetType().GetHashCode();
      server.ClientConnected += (o, e) => Interlocked.Increment(ref serverConnections);
      server.ClientDisconnected += (o, e) => Interlocked.Decrement(ref serverConnections);

      client.MessageReceived += (o, e) => clientStatus = e.Message;
      client.ExceptionOccurred += (o, e) => clientStatus = e.Exception.GetType().GetHashCode();
      client.Connected += (o, e) => Interlocked.Increment(ref clientConnections);
      client.Disconnected += (o, e) => Interlocked.Decrement(ref clientConnections);

      await server.StartAsync();

      await client.ConnectAsync();

      await Task.Delay(10);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);

      await server.WriteAsync(21);
      await client.WriteAsync(37);

      await Task.Delay(10);

      serverStatus.Should().Be(37);
      clientStatus.Should().Be(21);

      await client.DisconnectAsync();

      await Task.Delay(4000);

      serverConnections.Should().Be(0);
      clientConnections.Should().Be(0);

      await client.ConnectAsync();

      await Task.Delay(1000);

      serverConnections.Should().Be(1);
      clientConnections.Should().Be(1);
    }
  }
}
