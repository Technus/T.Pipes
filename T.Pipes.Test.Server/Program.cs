using Pastel;

namespace T.Pipes.Test.Server
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      using (var server = new Server())
      {
        AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => server.Dispose();

        var task = server.StartAsync();
        _ = task.ContinueWith(static x => Environment.Exit(-1), TaskContinuationOptions.NotOnRanToCompletion);
        await task;

        using (var item = server.Create())
        {
          var target = item.Callback.AsIAbstract;
          var target1 = item.Callback.AsIAbstract1;

          Console.WriteLine($"{nameof(target.GetInt)} = {target.GetInt()}".Pastel(ConsoleColor.Green));
          Console.WriteLine($"{nameof(target1.GetT)} = {target1.GetT()}".Pastel(ConsoleColor.Green));
        }
        await Task.Delay(1000);
      }
      await Task.Delay(Timeout.Infinite);
    }
  }
}