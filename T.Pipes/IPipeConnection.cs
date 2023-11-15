using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  public interface IPipeConnection<T> : IAsyncDisposable, IDisposable
  {
    Task WriteAsync(T value, CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    bool IsRunning { get; }
    string PipeName { get; }
    string ServerName { get; }
  }
}
