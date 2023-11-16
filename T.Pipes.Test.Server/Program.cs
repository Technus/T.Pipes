using System.Diagnostics;
using Pastel;

namespace T.Pipes.Test.Server
{
  internal class Program
  {
    static void Main(string[] args)
    {
#if DEBUG
      Debugger.Launch();
#endif

      var server = new Server();

      AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => server.Dispose();

      Task.Run(server.StartAsync).ContinueWith(x => Environment.Exit(-1), TaskContinuationOptions.NotOnRanToCompletion);

      Task.Delay(Timeout.Infinite).Wait();
    }
  }
}