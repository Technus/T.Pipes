using T.Pipes.Test.Abstractions;
using T.Pipes.Abstractions;

namespace T.Pipes.Test.Server
{
  internal static class Program
  {
    public static void Main(string[] args) => Start().Wait();

    private static async Task Start()
    {
      $"Server Core: {typeof(byte).Assembly.FullName}".WriteLine(ConsoleColor.Green);
#if !DEBUG
      await using var client = new SurrogateProcessWrapper(new(PipeConstants.ClientExeName));
      await client.StartProcess();
#endif
      await using (var server = new Server())
      {
        await server.StartAndConnectWithTimeoutAsync(PipeConstants.ConnectionAwaitTimeMs);

        await using (var item = await server.Callback.Create())
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
          catch (NoResponseException ex)
          {
            ex.ToString().WriteLine(ConsoleColor.Cyan, ConsoleColor.DarkGray);// Pipe errors
          }
          catch (Exception ex)
          {
            ex.ToString().WriteLine(ConsoleColor.Yellow, ConsoleColor.DarkGray);// Remote Target errors
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

    private static void Target_Get(string obj) => obj.WriteLine(ConsoleColor.Green);
  }
}