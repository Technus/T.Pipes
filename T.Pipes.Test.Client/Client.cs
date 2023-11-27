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
      Console.WriteLine((PipeConstants.ClientDisplayName + " Start").Pastel(ConsoleColor.Yellow));
      var startTask = Pipe.StartAsync();
      using var cts = new CancellationTokenSource();
      if (await Task.WhenAny(startTask, Task.Delay(PipeConstants.ConnectionAwaitTimeMs, cts.Token)) == startTask)
      {
        cts.Cancel();
        Console.WriteLine((PipeConstants.ClientDisplayName+" Connected").Pastel(ConsoleColor.Yellow));
        return;
      }
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the server was not started or connection was impossible".Pastel(ConsoleColor.DarkYellow));
    }

    private class Callback : IPipeCallback<PipeMessage>
    {
      private readonly Dictionary<string, IPipeDelegatingConnection<PipeMessage>> _mapping = new();

      public void Connected(string connection) => Clear();

      public void Disconnected(string connection)
      {
        Clear();
        throw new InvalidOperationException(("Disconnected occurred in "+PipeConstants.ClientDisplayName).Pastel(ConsoleColor.DarkYellow));
      }

      private void Clear()
      {
        foreach (var client in _mapping.Values)
        {
          client.Dispose();
        }
        _mapping.Clear();
      }

      public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

      public ValueTask DisposeAsync()
      {
        Clear();
        return default;
      }

      public void OnExceptionOccurred(Exception e)
      {
        Console.WriteLine(e.ToString().Pastel(ConsoleColor.DarkYellow));
        Clear();
      }

      public async void OnMessageReceived(PipeMessage command)
      {
        Console.WriteLine(("I: "+command.ToString()).Pastel(ConsoleColor.Yellow));
        //if (message is null)
        //{
        //  return;
        //}

        var proxy = CreateRequestedObjectProxy(command);

        var failedOnce = proxy.Callback.FailedOnce;

        _ = failedOnce.ContinueWith(async x =>
        {
          _mapping.Remove(proxy.ServerName);
          await proxy.DisposeAsync();
          proxy.Callback.Target.Dispose();
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        _ = failedOnce.ContinueWith(x =>
        {
          _mapping.Remove(proxy.ServerName);
          proxy.Callback.Target.Dispose();
        }, TaskContinuationOptions.OnlyOnCanceled);

        var startTask = proxy.StartAsync();
        using var cts = new CancellationTokenSource();
        if (await Task.WhenAny(startTask, Task.Delay(PipeConstants.ConnectionAwaitTimeMs, cts.Token)) == startTask)
        {
          cts.Cancel();
          _mapping.Add(command.Parameter!.ToString()!, proxy);
          return;
        }
        proxy.Dispose();
        proxy.Callback.Target.Dispose();
        throw new InvalidOperationException($"The {nameof(command)}: {command.Command}, could not be performed, connection timed out.".Pastel(ConsoleColor.DarkYellow));
      }

      public static IPipeDelegatingConnection<PipeMessage> CreateRequestedObjectProxy(PipeMessage command) => command.Command switch
      {
        PipeConstants.Create => new DelegatingClientAuto<Target>(command.Parameter!.ToString()!, new Target()),
        _ => throw new ArgumentException($"Invalid command: {command}".Pastel(ConsoleColor.DarkYellow), nameof(command)),
      };

      public void OnMessageSent(PipeMessage message) => Console.WriteLine(("O: " + message.ToString()).Pastel(ConsoleColor.Yellow));
    }
  }
}
