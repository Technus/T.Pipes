using System.Diagnostics;
using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  /// <summary>
  /// Main server used to control Delegating Server Instances
  /// </summary>
  internal class Server : IDisposable, IAsyncDisposable
  {
    private readonly Process _process = new() { StartInfo = new ProcessStartInfo(PipeConstants.ClientExeName) };
    private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = new();

    private PipeServer<Callback> Pipe { get; }

    public Server() => Pipe = new(PipeConstants.ServerName, new(this));

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
      await Pipe.DisposeAsync();
      await Pipe.Callback.DisposeAsync();
      foreach (var server in _mapping.Values)
      {
        await server.DisposeAsync();
      }
      _mapping.Clear();
      _process.Close();
      _process.Dispose();
    }

    public async Task StartAsync()
    {
      Console.WriteLine((PipeConstants.ServerDisplayName+" Start").Pastel(ConsoleColor.Cyan));
      await Pipe.StartAsync();
      _process.Start();
      var connectedTask = Pipe.Callback.ConnectedOnce;
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(connectedTask, Task.Delay(PipeConstants.ConnectionAwaitTimeMs, cts.Token)) == connectedTask)
      {
        cts.Cancel();
        Console.WriteLine((PipeConstants.ServerDisplayName+" Connected").Pastel(ConsoleColor.Cyan));
        return;
      }
      _process.Close();
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the client was not started or connection was impossible".Pastel(ConsoleColor.DarkCyan));
    }

    public void Clear()
    {
      foreach (var server in _mapping.Values)
      {
        server.Dispose();
      }
      _mapping.Clear();
    }

    private async Task<T> CreateInternal<T>(string command, T implementationServer) 
      where T : IPipeDelegatingConnection<PipeMessage>
    {
      var failedOnce = implementationServer.Callback.FailedOnce;

      _ = failedOnce.ContinueWith(x =>
      {
        _mapping.Remove(implementationServer.ServerName);
        _ = implementationServer.DisposeAsync();
      }, TaskContinuationOptions.OnlyOnRanToCompletion);

      _ = failedOnce.ContinueWith(x =>
        _mapping.Remove(implementationServer.ServerName), TaskContinuationOptions.OnlyOnCanceled);

      await implementationServer.StartAsync();
      _ = Pipe.WriteAsync(PipeMessageFactory.Instance.Create(command, implementationServer.ServerName));
      var connectedTask = implementationServer.Callback.ConnectedOnce;
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(connectedTask, Task.Delay(PipeConstants.ConnectionAwaitTimeMs)) == connectedTask)
      {
        cts.Cancel();
        _mapping.Add(implementationServer.ServerName, implementationServer);
        return implementationServer;
      }
      _ = implementationServer.DisposeAsync();
      throw new InvalidOperationException($"The {nameof(command)}: {command}, could not be performed, connection timed out.".Pastel(ConsoleColor.DarkCyan));
    }

    public Task<DelegatingServerAuto> CreateAsync() =>
      CreateInternal(PipeConstants.Create, new DelegatingServerAuto(Guid.NewGuid().ToString()));

    public DelegatingServerAuto Create() => CreateAsync().Result;

    internal class Callback : IPipeCallback<PipeMessage>
    {
      private readonly TaskCompletionSource<object?> _connectedOnce = new();
      private readonly Server _server;

      public Callback(Server server) => _server = server;

      public Task ConnectedOnce => _connectedOnce.Task;

      public ValueTask DisposeAsync()
      {
        _connectedOnce.TrySetCanceled();
        return default;
      }

      public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

      public void Connected(string connection)
      {
        _server.Clear();
        _connectedOnce.TrySetResult(null);
      }

      public void Disconnected(string connection)
      {
        _server.Clear();
        _connectedOnce.TrySetCanceled();
      }

      public void OnExceptionOccurred(Exception e)
      {
        Console.WriteLine(e.ToString()?.Pastel(ConsoleColor.DarkCyan));
        _server.Clear();
        _connectedOnce.TrySetException(e);
      }

      public void OnMessageReceived(PipeMessage message)
      {
        Console.WriteLine(("I: " + message.ToString()).Pastel(ConsoleColor.Cyan));
      }

      public void OnMessageSent(PipeMessage message)
      {
        Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Cyan));
      }
    }
  }
}
