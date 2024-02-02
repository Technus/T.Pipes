using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal static class Program
  {
    private static async Task Main(string[]? args)
    {
      $"Client Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.White);
      var name = args is null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0])
        ? throw new ArgumentOutOfRangeException(nameof(args), args, "Pipe name was invalid")
        : args[0];
      $"Pipe name: {name}".WriteLine(ConsoleColor.White);
      await using (var client = new Client(name))
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