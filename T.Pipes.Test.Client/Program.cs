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

      var client = new Client();

      AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => client.Dispose();

      await client.StartAsync()
        .ContinueWith(static async x => { await x; Environment.Exit(-1); }, TaskContinuationOptions.NotOnRanToCompletion); ;

      await Task.Delay(Timeout.Infinite);
    }
  }
}