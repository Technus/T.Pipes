using System.Diagnostics;
using Pastel;
using T.Pipes.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class Server
  {
    private readonly Process _process = new() { StartInfo = new ProcessStartInfo("T.Pipes.Test.Client.exe") };
    private PipeServer<Callback> Pipe { get; }

    public Server() => Pipe = new("T.Pipes.Test", new(this));

    public void Dispose() => DisposeAsync().AsTask().Wait();

    public async ValueTask DisposeAsync()
    {
      await Pipe.DisposeAsync();
      await Pipe.Callback.DisposeAsync();
      _process.Close();
      _process.Dispose();
    }

    public async Task StartAsync()
    {
      Console.WriteLine("T.Pipes.Test.Server".Pastel(ConsoleColor.Cyan));
      Pipe.StartAsync().Wait();
      _process.Start();
      if (Pipe.Callback.ConnectedOnce.Wait(10000))
      {
        return;
      }
      _process.Close();
      await Pipe.StopAsync();
      throw new InvalidOperationException($"Either the client was not started or connection was impossible");
    }

    internal class Callback : IPipeCallback<PipeMessage>
    {
      private readonly TaskCompletionSource<object?> _connectedOnce = new();
      private readonly Server server;

      public Callback(Server server) => this.server = server;

      public Task ConnectedOnce => _connectedOnce.Task;

      public ValueTask DisposeAsync()
      {
        _connectedOnce.TrySetCanceled();
        return new ValueTask();
      }

      public void Dispose() => DisposeAsync().AsTask().Wait();

      public void Connected(string connection)
      {
        _connectedOnce.TrySetResult(null);
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(-1) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(-10) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(1) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(10) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(0) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(((long)int.MinValue)) }).Wait();
        server.Pipe.WriteAsync(new() { Id = Guid.NewGuid(), Command = "Egg", Parameter = new IntPtr(((long)int.MaxValue)) }).Wait();
        //server.Pipe.WriteAsync(new("Egg", new IntPtr(((long)int.MinValue)-1))).Wait();//All fine will crash since client is forced to be x86
        //server.Pipe.WriteAsync(new("Egg", new IntPtr(((long)int.MaxValue)+1))).Wait();//All fine will crash since client is forced to be x86
      }

      public void Disconnected(string connection) => _connectedOnce.TrySetCanceled();

      public void OnExceptionOccurred(Exception e)
      {
        Console.WriteLine(e.ToString()?.Pastel(ConsoleColor.Cyan));
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
