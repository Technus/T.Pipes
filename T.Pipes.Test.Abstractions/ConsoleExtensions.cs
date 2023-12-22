namespace T.Pipes.Test.Abstractions
{
  public static class ConsoleExtensions
  {
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public static void WriteLine(this string s, ConsoleColor? fg = default, ConsoleColor? bg = default)
    {
      _semaphore.Wait();
      try
      {
        if (fg.HasValue)
          Console.ForegroundColor = fg.Value;
        if (bg.HasValue)
          Console.BackgroundColor = bg.Value;
        Console.WriteLine(s);
        Console.ResetColor();
      }
      finally
      {
        _semaphore.Release();
      }
    }

    public static void Write(this string s, ConsoleColor? fg = default, ConsoleColor? bg = default)
    {
      _semaphore.Wait();
      try
      {
        if (fg.HasValue)
          Console.ForegroundColor = fg.Value;
        if (bg.HasValue)
          Console.BackgroundColor = bg.Value;
        Console.Write(s);
        Console.ResetColor();
      }
      finally
      {
        _semaphore.Release();
      }
    }
  }
}
