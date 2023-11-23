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

    public void Dispose() => DisposeAsync().AsTask().Wait();

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
      Console.WriteLine(PipeConstants.ServerDisplayName.Pastel(ConsoleColor.Cyan));
      Pipe.StartAsync().Wait();
      _process.Start();
      if (Pipe.Callback.ConnectedOnce.Wait(PipeConstants.ConnectionAwaitTimeMs))
      {
        return;
      }
      _process.Close();
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the client was not started or connection was impossible");
    }

    public void Clear()
    {
      foreach (var server in _mapping.Values)
      {
        server.Dispose();
      }
      _mapping.Clear();
    }

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

      public void Dispose() => DisposeAsync().AsTask().Wait();

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
        Console.WriteLine(e.ToString()?.Pastel(ConsoleColor.Cyan));
        _server.Clear();
        _connectedOnce.TrySetException(e);
      }

      public void OnMessageReceived(PipeMessage message)
      {
        Console.WriteLine(message.ToString()?.Pastel(ConsoleColor.DarkYellow));
      }

      public void OnMessageSent(PipeMessage message)
      {
        Console.WriteLine(message.ToString()?.Pastel(ConsoleColor.Cyan));
      }
    }
  }
}
