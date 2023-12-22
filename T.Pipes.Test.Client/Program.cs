using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal static class Program
  {
    public static void Main(string[] args) => Start().Wait();

    private static async Task Start()
    {
      $"Client Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.Green);
      await using (var client = new Client())
      {
        try
        {
          await client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(PipeConstants.ConnectionAwaitTimeMs, client.Callback.LifetimeCancellation);
        }
        catch
        {
          //Ignores
        }

        await Task.Delay(1000);
      }
      await Task.Delay(5000);
    }
  }
}