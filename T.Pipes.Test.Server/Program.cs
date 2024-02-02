using T.Pipes.Test.Abstractions;
using T.Pipes.Abstractions;

namespace T.Pipes.Test.Server
{
  internal static class Program
  {
    private static async Task Main(string[]? args)
    {
      $"Server Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.White);
      var name = args is null || args.Length==0 || string.IsNullOrEmpty(args[0])
        ? $"{PipeConstants.ServerPipeName}-{Guid.NewGuid()}"
        : args[0];
      $"Pipe name: {name}".WriteLine(ConsoleColor.White);
      await using (var server = new Server(name))
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