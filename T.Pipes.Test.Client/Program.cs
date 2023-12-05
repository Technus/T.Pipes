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

      await client.StartAndConnectWithTimeoutAsync(PipeConstants.ConnectionAwaitTimeMs, client.Callback.LifetimeCancellation);
      try
      {
        await Task.Delay(Timeout.Infinite, client.Callback.LifetimeCancellation);
      }
      catch (OperationCanceledException e) when (e.CancellationToken == client.Callback.LifetimeCancellation) { }
    }
  }
}