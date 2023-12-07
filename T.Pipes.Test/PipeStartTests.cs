namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeStartTests
  {
    [Fact]
    public async Task PipeClientStartAndFallTrough()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      using var cts = new CancellationTokenSource();
      cts.Cancel();
      var token = cts.Token;

      var action = () => client.StartAsync(token);

      await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task PipeServerStartAndFallTrough()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      using var cts = new CancellationTokenSource();
      cts.Cancel();
      var token = cts.Token;

      var action = () => server.StartAsync(cancellationToken: token);

      await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task PipeClientCancel()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      using var cts = new CancellationTokenSource();
      cts.Cancel();
      var token = cts.Token;

      var action = () => client.StartAndConnectAsync(token);

      await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCancel()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      using var cts = new CancellationTokenSource();
      cts.Cancel();
      var token = cts.Token;

      var action = () => server.StartAndConnectAsync(cancellationToken: token);

      await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientCancelAfter()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);
      var token = cts.Token;

      var action = () => client.StartAndConnectAsync(token);

      await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCancelAfter()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      using var cts = new CancellationTokenSource();
      cts.CancelAfter(100);
      var token = cts.Token;

      var action = () => server.StartAndConnectAsync(cancellationToken: token);

      await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientTimeout()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var action = () => client.StartAndConnectWithTimeoutAsync(timeoutMs: 100);

      await action.Should().ThrowWithinAsync<TimeoutException>(TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public async Task PipeServerTimeout()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAsync(timeoutMs: 100);

      await action.Should().ThrowWithinAsync<TimeoutException>(TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public async Task PipeClientConnectWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var action = () => client.StartAndConnectAsync();

      await action.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeServerConnectWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectAsync();

      await action.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeClientConnectWait2()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var action = () => client.StartAndConnectWithTimeoutAsync(timeoutMs: 2000);

      await action.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeServerConnectWait2()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAsync(timeoutMs: 2000);

      await action.Should().NotCompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PipeClientCallbackLifetimeCancellationWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var cts = new CancellationTokenSource();
      cts.Cancel();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      clientCallback.LifetimeCancellation.Returns(cts.Token);

      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var action = () => client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000);

      var result = await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await result.And.ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCallbackLifetimeCancellationWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var cts = new CancellationTokenSource();
      cts.Cancel();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      serverCallback.LifetimeCancellation.Returns(cts.Token);

      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000);

      var result = await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await result.And.ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeClientCallbackLifetimeCancellationAfterWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var cts = new CancellationTokenSource();
      cts.CancelAfter(100);
      var token = cts.Token;

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      clientCallback.LifetimeCancellation.Returns(cts.Token);

      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      var task = Cache(() => client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000));
      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await task.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task PipeServerCallbackLifetimeCancellationAfterWait()
    {
      var pipeName = Guid.NewGuid().ToString();

      var cts = new CancellationTokenSource();
      cts.CancelAfter(100);

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      serverCallback.LifetimeCancellation.Returns(cts.Token);

      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAndAwaitCancellationAsync(timeoutMs: 2000);

      var result = await action.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(200));
      await result.And.ThrowAsync<OperationCanceledException>();
    }
  }
}
