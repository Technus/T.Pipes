using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal static class Program
  {
    private static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      await using (var server = new Server())
      {
        await server.StartProcess();
        await server.StartAndConnectWithTimeoutAsync(PipeConstants.ConnectionAwaitTimeMs);

        await using (var item = await server.Callback.Create())
        {
          var target = item.Callback.AsIAbstract;
          var target1 = item.Callback.AsIAbstract_args_Int16_end_;

          target1.GetT();
          target.GetInt();
        }
        await Task.Delay(1000);
      }
      await Task.Delay(5000);
    }
  }
}