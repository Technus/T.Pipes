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

    public Client() => Pipe = new(PipeConstants.ServerName, new(this));

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
      private Client client;

      public Callback(Client client) => this.client = client;

      public void Connected(string connection) { }
      public void Disconnected(string connection) { }
      public void Dispose() { }
      public ValueTask DisposeAsync() => default;
      public void OnExceptionOccurred(Exception e) => Console.WriteLine(e.ToString().Pastel(ConsoleColor.Yellow));

      public void OnMessageReceived(PipeMessage message)
      {
        Console.WriteLine(message.ToString().Pastel(ConsoleColor.DarkCyan));
        Task.Run(() => client.Pipe.WriteAsync(new() {
          Id = message.Id,
          Command = message.Command,
          Parameter = message.Parameter,
        }));
      }

      public void OnMessageSent(PipeMessage message) => Console.WriteLine(message.ToString().Pastel(ConsoleColor.Yellow));
    }
  }
}
