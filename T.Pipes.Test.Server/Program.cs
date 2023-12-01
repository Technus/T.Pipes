namespace T.Pipes.Test.Server
{
  internal class Program
  {
    private static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      await using (var server = new Server())
      {
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => server.Dispose();

        await server.StartAndConnectWithTimeoutAsync();

        await using (var item = server.Callback.Create())
        {
          var target = item.Callback.AsIAbstract;
          var target1 = item.Callback.AsIAbstract_args_Int16_end_;

          target.GetInt();
          target1.GetT();
        }
        await Task.Delay(1000);
      }
      await Task.Delay(Timeout.Infinite);
    }
  }
}