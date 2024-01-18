using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
  /// </summary>
  public abstract class SpawningPipeCallback
    : PipeCallbackBase<PipeMessage, PipeMessageFactory, SpawningPipeCallback>
  {
    private readonly Dictionary<long, TaskCompletionSource<object?>> _responses = new(16);
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Creates Callback to handle Factorization of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected SpawningPipeCallback(int responseTimeoutMs = Timeout.Infinite) : base(PipeMessageFactory.Instance)
      => ResponseTimeoutMs = responseTimeoutMs;

    /// <summary>
    /// The response await timeout should happen after that time
    /// </summary>
    public int ResponseTimeoutMs { get; set; }

    /// <summary>
    /// Controls if proxy should be created only after getting the response
    /// This allows to trade between memory performance vs speed when establishing proxy fails
    /// </summary>
    /// <remarks>For low traffic keep enabled, if timing is crucial only then disable</remarks>
    public bool LazyProxyCreation { get; set; } = true;

    /// <inheritdoc/>
    public override async Task WriteAsync(PipeMessage message, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);
      await Connection.WriteAsync(message, cts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public override void OnConnected(string connection)
    {
      try
      {
        _semaphore.Wait(LifetimeCancellation);
      }
      catch
      {
        return;
      }
      if (_responses.Count > 0)
      {
        var exception = new LocalNoResponseException("Connected", new InvalidOperationException("Connection occurred while operations were pending"));
        foreach (var item in _responses)
        {
          try
          {
            item.Value.TrySetException(exception);
          }
          finally
          {
            //Ignored
          }
        }
        _responses.Clear();
      }
      _semaphore.Release();
    }

    /// <summary>
    /// disposes created Proxies
    /// </summary>
    public override void OnDisconnected(string connection)
    {
      try
      {
        _semaphore.Wait(LifetimeCancellation);
      }
      catch
      {
        return;
      }
      if (_responses.Count > 0)
      {
        var exception = new LocalNoResponseException("Disconnected", new InvalidOperationException("Disconnection occurred while operations were pending"));
        foreach (var item in _responses)
        {
          try
          {
            item.Value.TrySetException(exception);
          }
          finally
          {
            //Ignored
          }
        }
        _responses.Clear();
      }
      _semaphore.Release();
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="IPipeCallback{TPacket}.Connection"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await _semaphore.WaitAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="IPipeCallback{TPacket}.Connection"/>
    /// </summary>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if (includeAsync)
        _semaphore.Wait();
      if (_responses.Count > 0)
      {
        var exception = new LocalNoResponseException("Disposing", CreateDisposingException());
        foreach (var item in _responses)
        {
          item.Value.TrySetException(exception);
        }
        _responses.Clear();
      }
      _semaphore.Dispose();
    }

    /// <summary>
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetException(Exception)"/>
    /// </summary>
    public void ClearResponses(Exception? exception = default)
    {
      try
      {
        _semaphore.Wait(LifetimeCancellation);
      }
      catch
      {
        return;
      }
      if (_responses.Count > 0)
      {
        if (exception is not LocalNoResponseException)
        {
          exception = new LocalNoResponseException("Clearing", exception ?? new InvalidOperationException("Clearing while operations were pending"));
        }
        foreach (var item in _responses)
        {
          try
          {
            item.Value.TrySetException(exception);
          }
          finally
          {
            //Ignored
          }
        }
        _responses.Clear();
      }
      _semaphore.Release();
    }

    /// <inheritdoc/>
    public override void OnExceptionOccurred(Exception exception)
    {
      try
      {
        _semaphore.Wait(LifetimeCancellation);
      }
      catch
      {
        return;
      }
      if (_responses.Count > 0)
      {
        exception = new LocalNoResponseException("Exception occurred", exception);
        foreach (var item in _responses)
        {
          try
          {
            item.Value.TrySetException(exception);
          }
          finally
          {
            //Ignored
          }
        }
        _responses.Clear();
      }
      _semaphore.Release();
    }

    /// <summary>
    /// No-op handling of sent messages
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessageSent(PipeMessage message) { }

    /// <summary>
    /// Handles creation of Proxies
    /// Calls <see cref="OnProvideProxyCommandAsync{T}(PipeMessage, CancellationToken)"/> with broad generic type
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);

      if ((message.PacketType & PacketType.Response) != 0)//Any response
      {
        try
        {
          await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch
        {
          return;
        }
        var exists = _responses.TryGetValue(message.Id, out var response);
        if (exists)
          _responses.Remove(message.Id);
        _semaphore.Release();
        if (exists)
        {
          if (message.PacketType == PacketType.ResponseFailure)
            response!.TrySetException((Exception)message.Parameter!);
          else if (message.PacketType == PacketType.ResponseCancellation)
          {
            if (message.Parameter is Exception ex)
              response!.TrySetException(ex);
            else
              response!.TrySetCanceled(default);
          }
          else
            response!.TrySetResult(message.Parameter);
        }
      }
      else if ((message.PacketType & PacketType.Command) != 0)//Any command
      {
        if (message.PacketType == PacketType.Command)
        {
          await OnProvideProxyCommandAsync<IPipeDelegatingConnection<PipeMessage>>(message, cts.Token).ConfigureAwait(false);
        }
        else
          throw new InvalidOperationException("Failure or Cancellation commands are not supported");
      }
      else OnUnknownMessage(message);
    }

    /// <summary>
    /// Called on Command to create Delegating Proxy
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task OnProvideProxyCommandAsync<T>(PipeMessage command, CancellationToken cancellationToken = default) 
      where T : class, IPipeDelegatingConnection<PipeMessage>
    {
      if (command.Parameter is not string name || name == string.Empty)
      {
        var ex = new ArgumentException("Name was not specified.", nameof(command));
        try
        {
          await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Invalid Pipe Name", ex)), default).ConfigureAwait(false);
          return;
        }
        catch (Exception e)
        {
          throw new AggregateException(e, ex);
        }
      }

      try
      {
        cancellationToken.ThrowIfCancellationRequested();
      }
      catch (Exception ex)
      {
        try
        {
          await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Cancelled", ex)), default).ConfigureAwait(false);
          return;
        }
        catch (Exception e)
        {
          throw new AggregateException(e, ex);
        }
      }

      try
      {
        LifetimeCancellation.ThrowIfCancellationRequested();
      }
      catch (Exception ex)
      {
        try
        {
          await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Disposed", CreateDisposedException(ex))), default).ConfigureAwait(false);
          return;
        }
        catch (Exception e)
        {
          throw new AggregateException(e, ex);
        }
      }

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);

      T? proxy = default;
      try
      {
        try
        {
          proxy = (T)CreateProxy(command.Command, name!);
          if (proxy is null)
            throw new InvalidOperationException("Crated proxy was null");
        }
        catch (Exception ex)
        {
          try
          {
            await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Creating proxy", ex)), default).ConfigureAwait(false);
            return;
          }
          catch (Exception e)
          {
            throw new AggregateException(e, ex);
          }
        }

        await WriteAsync(PacketFactory.CreateResponse(command), cts.Token).ConfigureAwait(false);

        try
        {
          await proxy.StartAndConnectWithTimeoutAsync(ResponseTimeoutMs, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          try
          {
            await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Connecting", ex)), default).ConfigureAwait(false);
            return;
          }
          catch (Exception e)
          {
            throw new AggregateException(e, ex);
          }
        }

        proxy = default;//So it wont get Disposed
      }
      finally
      {
        if(proxy is not null)
          await proxy.DisposeAsync().ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Wraps <see cref="RequestProxy{T}(string)"/> to return null on failures
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <returns></returns>
    public virtual T? RequestProxyOrDefault<T>(string command)
      where T : class, IPipeDelegatingConnection<PipeMessage>
    {
      try
      {
        return RequestProxy<T>(command);
      }
      catch
      {
        return default;
      }
    }

    /// <summary>
    /// Server logic to initialize Delegating Proxy
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public virtual T RequestProxy<T>(string command)
      where T : class, IPipeDelegatingConnection<PipeMessage>
    {
      try
      {
        return RequestProxyAsync<T>(command).Result;
      }
      catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)//unpacks the exception once
      {
        throw ae.InnerException!;
      }
    }

    /// <summary>
    /// Wraps <see cref="RequestProxyAsync{T}(string, CancellationToken)"/> to return null on failures
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<T?> RequestProxyOrDefaultAsync<T>(string command, CancellationToken cancellationToken = default)
      where T : class, IPipeDelegatingConnection<PipeMessage>
    {
      try
      {
        return await RequestProxyAsync<T>(command, cancellationToken).ConfigureAwait(false);
      }
      catch
      {
        return default;
      }
    }

#nullable disable

    /// <summary>
    /// Server logic to initialize Delegating Proxy
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<T> RequestProxyAsync<T>(string command, CancellationToken cancellationToken = default)
      where T : class, IPipeDelegatingConnection<PipeMessage>
    {
      try
      {
        cancellationToken.ThrowIfCancellationRequested();
      }
      catch (Exception ex)
      {
        throw new LocalNoResponseException("Cancelled", ex);
      }
      try
      {
        LifetimeCancellation.ThrowIfCancellationRequested();
      }
      catch (Exception ex)
      {
        throw new LocalNoResponseException("Cancelled", CreateDisposedException(ex));
      }

      var tcs = new TaskCompletionSource<object>();

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);

#if NET5_0_OR_GREATER
      await using var ctr = cts.Token.UnsafeRegister(static x =>
      {
        var state = ((TaskCompletionSource<object> tcs, CancellationToken cancellationToken, CancellationToken lifetimeCancellation))x;
        if (state.tcs.Task.IsCompleted || state.cancellationToken.IsCancellationRequested || state.lifetimeCancellation.IsCancellationRequested)
          return;
        state.tcs.TrySetException(new LocalNoResponseException("Cancelled", new TimeoutException("Timed out")));
      }, (tcs, cancellationToken, LifetimeCancellation)).ConfigureAwait(false);
#else
      using var ctr = cts.Token.Register(static x =>
        {
          var state = ((TaskCompletionSource<object> tcs, CancellationToken cancellationToken, CancellationToken lifetimeCancellation))x;
          if (state.tcs.Task.IsCompleted || state.cancellationToken.IsCancellationRequested || state.lifetimeCancellation.IsCancellationRequested)
            return;
          state.tcs.TrySetException(new LocalNoResponseException("Cancelled",new TimeoutException("Timed out")));
        }, (tcs, cancellationToken, LifetimeCancellation));
#endif

      var pipeName = Guid.NewGuid().ToString();
      var message = PacketFactory.CreateCommand(command, pipeName);
      var lazyProxyCreation = LazyProxyCreation;
      T proxy = default;

      try
      {
        if (tcs.Task.IsCompleted)
          await tcs.Task.ConfigureAwait(false);

        if(!lazyProxyCreation)
        {
          try
          {
            proxy = (T)CreateProxy(command, pipeName);
            if (proxy is null)
              throw new InvalidOperationException("Crated proxy was null");
          }
          catch (Exception ex)
          {
            var e = new LocalNoResponseException("Creating Proxy", ex);
            tcs.TrySetException(e);
            throw e;
          }

          if (tcs.Task.IsCompleted)
            await tcs.Task.ConfigureAwait(false);
        }

        try
        {
          await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
          try
          {
            _responses.Add(message.Id, tcs);
          }
          finally
          {
            _semaphore.Release();
          }
        }
        catch (Exception ex)
        {
          var e = new LocalNoResponseException("Queueing TaskCompletionSource", ex);
          tcs.TrySetException(e);
          throw e;
        }

        if (tcs.Task.IsCompleted)
          await tcs.Task.ConfigureAwait(false);

        try
        {
          await WriteAsync(message, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          var e = new LocalNoResponseException("Sending Command", ex);
          tcs.TrySetException(e);
          throw e;
        }

        await tcs.Task.ConfigureAwait(false);//Wait for response if all went ok then it means client is started

        if(lazyProxyCreation)
        {
          try
          {
            proxy = (T)CreateProxy(command, pipeName);
            if (proxy is null)
              throw new InvalidOperationException("Crated proxy was null");
          }
          catch (Exception ex)
          {
            var e = new LocalNoResponseException("Creating Proxy", ex);
            tcs.TrySetException(e);
            throw e;
          }
        }

        try
        {
          await proxy.StartAndConnectWithTimeoutAsync(ResponseTimeoutMs, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          var e = new LocalNoResponseException("Awaiting Connection", ex);
          tcs.TrySetException(e);
          throw e;
        }

        try
        {
          return proxy;
        }
        finally
        {
          proxy = default;//So it wont get Disposed
        }
      }
      finally
      {
        if (!tcs.Task.IsCompleted)
          tcs.TrySetException(new LocalNoResponseException("Finally block reached", new InvalidOperationException("Failed to finish gracefully.")));

        if (proxy is not null)
          await proxy.DisposeAsync().ConfigureAwait(false);

        try
        {
          await _semaphore.WaitAsync(LifetimeCancellation).ConfigureAwait(false);
          _responses.Remove(message.Id);
          _semaphore.Release();
        }
        catch
        {
          //Ignored
        }
      }
    }

#nullable restore

    /// <summary>
    /// Create instances of <see cref="IPipeDelegatingConnection{TMessage}"/>
    /// It will be later cast to final type or the base interface by <see cref="OnProvideProxyCommandAsync{T}(PipeMessage, CancellationToken)"/>
    /// It will be later initialized by <see cref="OnProvideProxyCommandAsync{T}(PipeMessage, CancellationToken)"/>
    /// Custom logic needs to be provided to dispose elsewhere.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="pipeName">name for the pipe</param>
    /// <returns></returns>
    protected abstract IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName);
  }
}
