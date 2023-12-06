using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <inheritdoc/>
  public abstract class SpawningPipeServer<TCallback> 
    : SpawningPipeServer<H.Pipes.PipeServer<PipeMessage>, TCallback>
    where TCallback : SpawningPipeServerCallback
  {
    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="client"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(string pipe, ProcessStartInfo client, TCallback callback) : base(new(pipe), client, callback)
    {
    }

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="client"></param>
    /// <param name="callback"></param>
    protected SpawningPipeServer(H.Pipes.PipeServer<PipeMessage> pipe, ProcessStartInfo client, TCallback callback) : base(pipe, client, callback)
    {
    }
  }


  /// <summary>
  /// Helper to wrap factorization of <see cref="T.Pipes.Abstractions.IPipeDelegatingConnection{TMessage}"/><br/>
  /// </summary>
  /// <typeparam name="TPipe"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class SpawningPipeServer<TPipe, TCallback> 
    : PipeServer<TPipe, PipeMessage, TCallback>
    where TCallback : SpawningPipeCallback<TPipe>
    where TPipe : H.Pipes.IPipeServer<PipeMessage>
  {
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Process _process;

    /// <summary>
    /// Creates a proxy producer and target requester server
    /// </summary>
    /// <param name="pipe"></param>
    /// <param name="client"></param>
    /// <param name="callback"></param>
    public SpawningPipeServer(TPipe pipe, ProcessStartInfo client, TCallback callback) : base(pipe, callback)
      => _process = new Process { StartInfo = client };

    /// <summary>
    /// starts the process and the pipe
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
      await StartProcess(cancellationToken).ConfigureAwait(false);
      await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// stops the process and the pipe
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
      await base.StopAsync(cancellationToken).ConfigureAwait(false);
      await StopProcess(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts the client process
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task StartProcess(CancellationToken cancellationToken = default)
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
    protected async Task StopProcess(CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(10000);
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
        var timeout = 10;
        while (!_process.WaitForExit(timeout))
        {
          if (cts.Token.IsCancellationRequested)
            throw new OperationCanceledException("Cancelled waiting for graceful close", cts.Token);
          await Task.Yield();
          if (timeout < 100)
            timeout += 10;
        }
#endif
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          if (closeSent)
          {
            try
            {
              _process.Kill();
            }
            catch { }
          }
          else throw;
        }
        else
        {
          try
          {
            _process.Kill();
          }
          catch { }
        }
      }
      catch
      {
        try
        {
          _process.Kill();
        }
        catch { }
      }
      finally
      {
        _semaphore.Release();
      }
    }

    /// <summary>
    /// Disposes <see cref="PipeConnection{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnection{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if (includeAsync)
      {
        Callback.Dispose();
        _semaphore.Wait();
      }
      try
      {
        _process.Kill();
      }
      catch { }
      finally
      {
        _process.Dispose();
      }
      _semaphore.Dispose();
    }

    /// <summary>
    /// Disposes <see cref="PipeConnection{TPipe, TPacket, TCallback}.Pipe"/> and <see cref="PipeConnection{TPipe, TPacket, TCallback}.Callback"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await Callback.DisposeAsync().ConfigureAwait(false);
      await _semaphore.WaitAsync().ConfigureAwait(false);
    }
  }
}
