namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeStartTests
  {
    [Fact]
    public async Task PipeClientStartAndFallTrough()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PipeServerStartAndFallTrough()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PipeClientCancel()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      using var cts = new CancellationTokenSource();
      cts.Cancel();

      var task = client.StartAndConnectAsync(cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCancel()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      using var cts = new CancellationTokenSource();
      cts.Cancel();

      var task = server.StartAndConnectAsync(cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientCancelAfter()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);

      var task = client.StartAndConnectAsync(cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(150));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCancelAfter()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);

      var task = server.StartAndConnectAsync(cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(150));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientTimeout()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAndConnectWithTimeoutAsync(timeoutMs: 100);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(150));
      task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task PipeServerTimeout()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAndConnectWithTimeoutAsync(timeoutMs: 100);

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(150));
      task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task PipeClientConnectWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAndConnectAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeServerConnectWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAndConnectAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeClientConnectWait2()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAndConnectWithTimeoutAsync(timeoutMs: 2000);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
      await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(2));
      task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task PipeServerConnectWait2()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAndConnectWithTimeoutAsync(timeoutMs: 2000);

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
      await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(2));
      task.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task PipeClientCallbackLifetimeCancellationWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      using var cts = new CancellationTokenSource();
      cts.Cancel();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      clientCallback.LifetimeCancellation.Returns(cts.Token);
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000, cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCallbackLifetimeCancellationWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      using var cts = new CancellationTokenSource();
      cts.Cancel();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      serverCallback.LifetimeCancellation.Returns(cts.Token);
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000, cts.Token);

      await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientCallbackLifetimeCancellationAfterWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      clientCallback.LifetimeCancellation.Returns(cts.Token);

      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = Cache(() => client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000, cts.Token));
      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCallbackLifetimeCancellationAfterWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);

      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      serverCallback.LifetimeCancellation.Returns(cts.Token);

      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000, cts.Token);

      var result = await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await result.And.ThrowAsync<OperationCanceledException>();
    
    }

    [Fact]
    public async Task PipeClientStartAndWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = client.StartAndConnectWithTimeoutAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeServerStartAndWait()
    {
      var pipeName = Guid.NewGuid().ToString();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var task = server.StartAndConnectWithTimeoutAsync();

      await task.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }
  }
}
