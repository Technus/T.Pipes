using Pastel;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  /// <summary>
  /// Main client used to control Delegating Client instances
  /// </summary>
  internal class Client : IDisposable, IAsyncDisposable
  {
    private PipeClient<Callback> Pipe { get; }

    public Client() => Pipe = new(PipeConstants.ServerName, new());

    public void Dispose() => DisposeAsync().AsTask().Wait();

    public async ValueTask DisposeAsync()
    {
      await Pipe.DisposeAsync();
      await Pipe.Callback.DisposeAsync();
    }

    public async Task StartAsync()
    {
      Console.WriteLine(PipeConstants.ClientDisplayName.Pastel(ConsoleColor.Yellow));
      if (Pipe.StartAsync().Wait(PipeConstants.ConnectionAwaitTimeMs))
      {
        //Connection ok
        return;
      }
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the server was not started or connection was impossible");
    }

    private class Callback : IPipeCallback<PipeMessage>
    {
      private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = new();

      public void Connected(string connection) => Clear();

      public void Disconnected(string connection)
      {
        Clear();
        throw new InvalidOperationException("Disconnected occurred in PipeClient");
      }

      private void Clear()
      {
        foreach (var client in _mapping.Values)
        {
          client.Dispose();
        }
        _mapping.Clear();
      }

      public void Dispose() => DisposeAsync().AsTask().Wait();

      public ValueTask DisposeAsync()
      {
        Clear();
        return default;
      }

      public void OnExceptionOccurred(Exception e)
      {
        Console.WriteLine(e.ToString().Pastel(ConsoleColor.Yellow));
        Clear();
      }

      public void OnMessageReceived(PipeMessage message)
      {
        Console.WriteLine(message.ToString().Pastel(ConsoleColor.DarkCyan));
        //if (message is null)
        //{
        //  return;
        //}

        var proxy = CreateRequestedObjectProxy(message.Command);

        Task.Run(() => proxy.Callback.FailedOnce).ContinueWith(async x =>
        {
          _mapping.Remove(proxy.ServerName);
          await proxy.DisposeAsync();
          proxy.Callback.Target.Dispose();
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
        if (proxy.StartAsync().Wait(PipeConstants.ConnectionAwaitTimeMs))
        {
          _mapping.Add(message.Parameter!.ToString()!, proxy);
          return;
        }
        proxy.Dispose();
        proxy.Callback.Target.Dispose();
        throw new InvalidOperationException($"The {nameof(message)}: {message.Command}, could not be performed, connection timed out.");
      }

      public static IPipeDelegatingConnection<PipeMessage> CreateRequestedObjectProxy(string command) => command switch
      {
        PipeConstants.Create => new DelegatingClientAuto<Target>(Guid.NewGuid().ToString(), new Target()),
        _ => throw new ArgumentException($"Invalid command: {command}", nameof(command)),
      };

      public void OnMessageSent(PipeMessage message) => Console.WriteLine(message.ToString().Pastel(ConsoleColor.Yellow));
    }
  }
}
