using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal static class Program
  {
    public static void Main(string[] args) => Start().Wait();

    private static async Task Start()
    {
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
            target1.GetT();
            target.GetInt();
          }
          catch
          {
            //Ignores
          }
        }
        await Task.Delay(1000);
      }
      await Task.Delay(5000);
#if !DEBUG
      await client.StopProcess();
#endif
    }
  }
}