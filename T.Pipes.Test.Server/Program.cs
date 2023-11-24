using System.Diagnostics;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      var server = new Server();

      AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => server.Dispose();

      var task = server.StartAsync();
      _ = task.ContinueWith(static x => Environment.Exit(-1), TaskContinuationOptions.NotOnRanToCompletion);
      await task;

      var item = server.Create();

      var target = item.Callback as IAbstract;
      var target1 = item.Callback as IAbstract<int>;

      var papa = target.GetInt();


      await item.DisposeAsync();

      await Task.Delay(Timeout.Infinite);
    }
  }
}