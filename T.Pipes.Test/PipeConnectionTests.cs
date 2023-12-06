using System.Diagnostics.CodeAnalysis;
using T.Pipes.Abstractions;

namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipeConnectionTests
  {
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

      await action.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task PipeServerTimeout()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var action = () => server.StartAndConnectWithTimeoutAsync(timeoutMs: 100);

      await action.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task PipeClientStartAndConnect()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      await server.StartAsync();

      await client.StartAndConnectAsync();
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

      await server.StartAndConnectAsync();
    }

    [Fact(Skip = "IsConnecting is very unreliable in H.Pipes")]
    [Obsolete("IsConnecting is very unreliable in H.Pipes")]
    public async Task PipeClientStartAndConnect1()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();

      await server.StartAsync();

      //server.IsConnecting.Should().BeTrue();
      client.IsConnecting.Should().BeFalse();

      await client.StartAndConnectAsync();

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();
    }

    [Fact(Skip = "IsConnecting is very unreliable in H.Pipes")]
    [Obsolete("IsConnecting is very unreliable in H.Pipes")]
    public async Task PipeServerStartAndConnect1()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();

      await client.StartAsync();

      server.IsConnecting.Should().BeFalse();
      //client.IsConnecting.Should().BeTrue();

      await server.StartAndConnectAsync();

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();
    }

    [Fact(Skip = "IsConnecting is very unreliable in H.Pipes")]
    [Obsolete("IsConnecting is very unreliable in H.Pipes")]
    public async Task PipeClientStartAndConnect2()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();

      await Task.Yield();
      await server.StartAsync();
      await Task.Yield();

      //server.IsConnecting.Should().BeTrue();
      client.IsConnecting.Should().BeFalse();

      await Task.Yield();
      await client.StartAndConnectAsync();
      await Task.Yield();

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();
    }

    [Fact(Skip = "IsConnecting is very unreliable in H.Pipes")]
    [Obsolete("IsConnecting is very unreliable in H.Pipes")]
    public async Task PipeServerStartAndConnect2()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();

      await Task.Yield();
      await client.StartAsync();
      await Task.Yield();

      server.IsConnecting.Should().BeFalse();
      //client.IsConnecting.Should().BeTrue();

      await Task.Yield();
      await server.StartAndConnectAsync();
      await Task.Yield();

      server.IsConnecting.Should().BeFalse();
      client.IsConnecting.Should().BeFalse();
    }
  }
}