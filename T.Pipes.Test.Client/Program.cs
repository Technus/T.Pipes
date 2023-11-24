using System.Diagnostics;

namespace T.Pipes.Test.Client
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      using (var client = new Client())
      {
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => client.Dispose();

        var task = client.StartAsync();
        _ = task.ContinueWith(static x => Environment.Exit(-1), TaskContinuationOptions.NotOnRanToCompletion);
        await task;

        await Task.Delay(Timeout.Infinite);
      }
    }
  }
}