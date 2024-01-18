using T.Pipes.Test.Abstractions;
using T.Pipes.Abstractions;

namespace T.Pipes.Test.Server
{
  internal static class Program
  {
    public static void Main(string[] args) => Start().Wait();

    private static async Task Start()
    {
      $"Server Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.White);
#if !DEBUG
      await using var client = new SurrogateProcessWrapper(new(PipeConstants.ClientExeName));
      await client.StartProcess();
#endif
      await using (var server = new Server())
      {
        await server.StartAndConnectWithTimeoutAsync(PipeConstants.ConnectionAwaitTimeMs);

        try
        {
          await using (var itemMaybe = await server.Callback.CreateInvalidAsync())
          {
            itemMaybe?.Callback?.AsIAbstract?.GetInt().ToString().WriteLine(ConsoleColor.Green);
          }
        }
        catch (Exception ex)
        {
          ex.PrintNicely();
        }

        using (var itemMaybe = server.Callback.CreateInvalidOrDefault())
        {
          itemMaybe?.GetInt().ToString().WriteLine(ConsoleColor.Green);
        }


        await using (var item = await server.Callback.CreateAsync())
        {
          var target = item.Callback.AsIAbstract;
          var target1 = item.Callback.AsIAbstract_args_Int16_end_;
          try
          {
            target1.GetT().ToString().WriteLine(ConsoleColor.Green);

            target.Get += Target_Get;
            target.GetStrings().ToString().WriteLine(ConsoleColor.Green);
            target.GetInt().ToString().WriteLine(ConsoleColor.Green);
          }
          catch (Exception ex)
          {
            ex.PrintNicely();
          }
          finally
          {
            target.Get -= Target_Get;
          }
        }
        await Task.Delay(1000);
      }
      await Task.Delay(10000);
#if !DEBUG
      await client.StopProcess();
#endif
    }

    internal static void PrintNicely(this Exception ex)
    {
      if (ex is RemoteNoResponseException)
        ("R: " + ex.ToString()).WriteLine(ConsoleColor.Yellow, ConsoleColor.DarkGray);// Pipe related errors
      else if (ex is LocalNoResponseException)
        ("L: " + ex.ToString()).WriteLine(ConsoleColor.Cyan, ConsoleColor.DarkGray);// Pipe related errors
      else
        ("X: " + ex.ToString()).WriteLine(ConsoleColor.Red);// Remote Target errors
    }

    private static void Target_Get(string obj) => obj.WriteLine(ConsoleColor.DarkGreen);
  }
}