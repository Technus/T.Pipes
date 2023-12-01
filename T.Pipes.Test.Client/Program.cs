using System.Diagnostics;

namespace T.Pipes.Test.Client
{
  internal class Program
  {
    private static async Task Main(string[] args)
    {
#if DEBUG
      Debugger.Launch();
#endif

      await using (var client = new Client())
      {
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => client.Dispose();

        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
      }
    }
  }
}