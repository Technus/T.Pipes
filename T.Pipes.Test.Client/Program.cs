using System.Diagnostics;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class Program
  {
    private static async Task Main(string[] args)
    {
#if DEBUG
      Debugger.Launch();
#endif

      await using var client = new Client();

      await client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(PipeConstants.ConnectionAwaitTimeMs);
    }
  }
}