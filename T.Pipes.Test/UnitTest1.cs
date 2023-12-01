namespace T.Pipes.Test
{
  public class UnitTest1
  {
    [Fact]
    public void Test1()
    {
      Wtf().Wait();
    }

    private static async Task Wtf()
    {
      var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);


      try
      {
        tcs.SetCanceled();
        await tcs.Task;
      }
      catch
      {

      }
    } 
  }
}