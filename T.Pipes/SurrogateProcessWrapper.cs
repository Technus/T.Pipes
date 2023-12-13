﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Wrapper for safe handling of surrogate process
  /// </summary>
  public class SurrogateProcessWrapper : BaseClass
  {
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// The process
    /// </summary>
    protected internal readonly Process _process;

    /// <summary>
    /// Constructs wrapper for <see cref="Process"/> using <paramref name="processStartInfo"/>
    /// </summary>
    /// <param name="processStartInfo"></param>
    public SurrogateProcessWrapper(ProcessStartInfo processStartInfo)
      => _process = new Process { StartInfo = processStartInfo };

    /// <summary>
    /// Timeout in ms to let process terminate on it's own
    /// </summary>
    public int ProcessKillTimeoutMs { get; set; } = 10000;

    /// <summary>
    /// Starts the client process
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartProcess(CancellationToken cancellationToken = default)
    {
      await StopProcess(cancellationToken).ConfigureAwait(false);
      await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      try
      {
        _process.Start();
      }
      finally
      {
        _semaphore.Release();
      }
    }

    /// <summary>
    /// Stops the client process, kills after 10s
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopProcess(CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      cts.CancelAfter(ProcessKillTimeoutMs);
      await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      var closeSent = false;
      try
      {
        _process.Refresh();
        if (_process.HasExited)
          return;

        closeSent = _process.CloseMainWindow();

#if NET5_0_OR_GREATER
        await _process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
#else
        var timeout = 1;
        while (!_process.WaitForExit(timeout))
        {
          if (cts.Token.IsCancellationRequested)
            cts.Token.ThrowIfCancellationRequested();
          await Task.Yield();
          if (timeout < 100)
            timeout += 1;
        }
#endif
      }
      catch
      {
        if (cancellationToken.IsCancellationRequested && !closeSent)
        {
          throw;
        }
        else
        {
          try
          {
            _process.Kill();
          }
          catch
          {
            //Should be done anyway at this point
          }
        }
      }
      finally
      {
        _semaphore.Release();
      }
    }    
    
    /// <summary>
    /// Disposes <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if (includeAsync)
      {
        _semaphore.Wait();
      }
      try
      {
        _process.Kill();
      }
      catch
      {
        //Should be done anyway at this point
      }
      finally
      {
        _process.Dispose();
      }
      _semaphore.Dispose();
    }

    /// <summary>
    /// Disposes <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnectionBase{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await _semaphore.WaitAsync().ConfigureAwait(false);
    }
  }
}
