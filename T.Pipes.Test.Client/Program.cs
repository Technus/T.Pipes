using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal static class Program
  {
    public static void Main(string[] args) => Start().Wait();

    private static async Task Start()
    {
      $"Client Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.White);
      await using (var client = new Client())
      {
        try
        {
          await client.StartAndConnectWithTimeoutAndAwaitCancellationAsync(PipeConstants.ConnectionAwaitTimeMs);
        }
        catch (Exception e)
        {
          e.PrintNicely();
        }

        await Task.Delay(1000);
      }
      await Task.Delay(10000);
    }

    internal static void PrintNicely(this Exception ex)
    {
      if (ex is RemoteNoResponseException)
        ("R: " + ex.ToString()).WriteLine(ConsoleColor.Cyan, ConsoleColor.DarkGray);// Pipe related errors
      else if (ex is LocalNoResponseException)
        ("L: " + ex.ToString()).WriteLine(ConsoleColor.Yellow, ConsoleColor.DarkGray);// Pipe related errors
      else
        ("X: " + ex.ToString()).WriteLine(ConsoleColor.Red);// Remote Target errors
    }
  }
}