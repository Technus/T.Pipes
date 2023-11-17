using System.Diagnostics;

namespace T.Pipes.Test.Client
{
  internal class Program
  {
    static void Main(string[] args)
    {
#if DEBUG
      //Debugger.Launch();
#endif

      var client = new Client();

      AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) => client.Dispose();

      Task.Run(client.StartAsync).ContinueWith(x => Environment.Exit(-1), TaskContinuationOptions.NotOnRanToCompletion);

      Task.Delay(Timeout.Infinite).Wait();
    }
  }
}