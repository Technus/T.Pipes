using System;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  public interface IPipeConnection<TMessage> : IAsyncDisposable, IDisposable
  {
    Task WriteAsync(TMessage value, CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    bool IsRunning { get; }
    string PipeName { get; }
    string ServerName { get; }
  }
}
