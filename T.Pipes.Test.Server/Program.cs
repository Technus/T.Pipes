using System.Diagnostics;

namespace T.Pipes.Test.Server
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      var server = new Server();

      AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => server.Dispose();

      await server.StartAsync()
        .ContinueWith(static async x => { await x; Environment.Exit(-1); }, TaskContinuationOptions.NotOnRanToCompletion);

      var item = server.Create();

      await item.DisposeAsync();

      await Task.Delay(Timeout.Infinite);
    }
  }
}