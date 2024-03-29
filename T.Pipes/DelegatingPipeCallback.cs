using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Callback for original implementation of <typeparamref name="TTarget"/> for:<br/>
  /// <see cref="DelegatingPipeClient{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TTarget">target to operate on</typeparam>
  /// <typeparam name="TCallback">the final implementing type</typeparam>
  public abstract class DelegatingPipeClientCallback<TTarget, TCallback>
    : DelegatingPipeCallback<TTarget, TCallback>
    where TCallback : DelegatingPipeClientCallback<TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeClientCallback(TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(target, responseTimeoutMs)
    {
    }
  }

  /// <summary>
  /// Callback and proxy implementation of <typeparamref name="TTargetAndCallback"/> for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TTargetAndCallback">target to operate on and the callback at the same time</typeparam>
  public abstract class DelegatingPipeServerCallback<TTargetAndCallback>
    : DelegatingPipeCallback<TTargetAndCallback, TTargetAndCallback>
    where TTargetAndCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTargetAndCallback, TTargetAndCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeServerCallback(int responseTimeoutMs = Timeout.Infinite) : base(responseTimeoutMs)
    {
    }
  }

  /// <summary>
  /// Callback and proxy implementation of <typeparamref name="TTarget"/> for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TTarget">target to operate on</typeparam>
  /// <typeparam name="TCallback">the final implementing type</typeparam>
  public abstract class DelegatingPipeServerCallback<TTarget, TCallback>
    : DelegatingPipeCallback<TTarget, TCallback>
    where TCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeServerCallback(int responseTimeoutMs = Timeout.Infinite) : base(responseTimeoutMs)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeCallback<TTarget, TCallback>
    : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeCallback(int responseTimeoutMs = Timeout.Infinite) : base(PipeMessageFactory.Instance, responseTimeoutMs)
    {
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    /// <param name="responseTimeoutMs"></param>
    protected DelegatingPipeCallback(TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(PipeMessageFactory.Instance, target, responseTimeoutMs)
    {
    }
  }

  /// <summary>
  /// Base callback implementation for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/><br/>
  /// <see cref="DelegatingPipeClient{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TPacket"><see cref="IPipeMessage"/></typeparam>
  /// <typeparam name="TPacketFactory"><see cref="IPipeMessageFactory{TPacket}"/></typeparam>
  /// <typeparam name="TTarget">target to operate on</typeparam>
  /// <typeparam name="TCallback">the final implementing type</typeparam>
  public abstract class DelegatingPipeCallback<TPacket, TPacketFactory, TTarget, TCallback>
    : PipeCallbackBase<TPacket, TPacketFactory, TCallback>, IPipeDelegatingCallback<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPacket, TPacketFactory, TTarget, TCallback>
  {
    /// <summary>
    /// A Command function definition
    /// </summary>
    /// <param name="callback">associated callback/pipe/packetFactory</param>
    /// <param name="message">message received</param>
    /// <param name="cancellationToken"></param>
    /// <returns>return value/s from target</returns>
    public delegate Task CommandFunction(TCallback callback, TPacket message, CancellationToken cancellationToken);

    private readonly Dictionary<long, TaskCompletionSource<object?>> _responses = new(16);
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private TTarget _target;

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <param name="responseTimeoutMs">response timeout in ms</param>
    /// <exception cref="InvalidOperationException">when the this is not a valid <typeparamref name="TTarget"/> or null</exception>
    protected DelegatingPipeCallback(TPacketFactory packetFactory, int responseTimeoutMs = Timeout.Infinite) : base(packetFactory)
    {
      ResponseTimeoutMs = responseTimeoutMs;

#pragma warning disable S3060 // "is" should not be used with "this"
      if (this is TTarget tt)
        Target = tt;
#pragma warning restore S3060 // "is" should not be used with "this"

      if (_target is null)
        throw new InvalidOperationException($"{GetType().FullName} is not {typeof(TTarget).FullName}");
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    /// <param name="responseTimeoutMs">response timeout in ms</param>
    /// <exception cref="InvalidOperationException">when <paramref name="target"/> is null</exception>
    protected DelegatingPipeCallback(TPacketFactory packetFactory, TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(packetFactory)
    {
      ResponseTimeoutMs = responseTimeoutMs;

      if (target is not null)
        Target = target;

      if (_target is null)
        throw new InvalidOperationException($"Target is not {typeof(TTarget).FullName}");
    }

    /// <summary>
    /// Static Collection of Command functions
    /// </summary>
#pragma warning disable S2743 // Static fields should not be used in generic types but it is fine since we expect the commands to be auto generated.
    public static Dictionary<string, CommandFunction> Functions { get; } = [];
#pragma warning restore S2743 // Static fields should not be used in generic types but it is fine since we expect the commands to be auto generated.

    /// <summary>
    /// For client an instance of <typeparamref name="TTarget"/>
    /// For server returns 'this'
    /// </summary>
    public TTarget Target
    {
      get => _target;
      protected set
      {
        if (Target is not null)
        {
          TargetDeInitAuto();
          TargetDeInit(Target);
        }

        _semaphore.Wait(LifetimeCancellation);
        try
        {
          if (_responses.Count > 0)
          {
            var exception = new LocalNoResponseException("Changing target while operations are pending", new InvalidOperationException("Changed Target"));
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
        }
        finally
        {
          _semaphore.Release();
        }

        _target = value;

        if (Target is not null)
        {
          TargetInitAuto();
          TargetInit(Target);
        }
        else
        {
          throw new InvalidOperationException($"Target is not {typeof(TTarget).FullName}");
        }
      }
    }

    /// <summary>
    /// Initialization logic for new target
    /// </summary>
    /// <param name="target">the target to initialize against</param>
    protected virtual void TargetInit(TTarget target) { }

    /// <summary>
    /// DeInitialization logic for old target
    /// </summary>
    /// <param name="target">the target to de-initialize against</param>
    protected virtual void TargetDeInit(TTarget target) { }

    /// <summary>
    /// Stub for source generator, to hook events
    /// </summary>
    protected virtual void TargetInitAuto() { }

    /// <summary>
    /// Stub for source generator, to unhook events
    /// </summary>
    protected virtual void TargetDeInitAuto() { }

    /// <summary>
    /// Stub for source generator, to dispose the generated events by setting them null
    /// </summary>
    protected virtual void TargetDisposeAuto() { }

    /// <inheritdoc/>
    public int ResponseTimeoutMs { get; set; }

    /// <inheritdoc/>
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
      try
      {
        if (_responses.Count > 0)
        {
          var exception = new LocalNoResponseException("Connection occurred while operations are pending", new InvalidOperationException($"Connected: {connection}"));
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
      }
      finally
      {
        _semaphore.Release();
      }
    }

    /// <inheritdoc/>
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
      try
      {
        if (_responses.Count > 0)
        {
          var exception = new LocalNoResponseException("Disconnection occurred while operations are pending", new InvalidOperationException($"Disconnected: {connection}"));
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
      }
      finally
      {
        _semaphore.Release();
      }
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="IPipeCallback{TPacket}.Connection"/> nor the <see cref="Target"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await _semaphore.WaitAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="IPipeCallback{TPacket}.Connection"/> nor the <see cref="Target"/>
    /// </summary>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if(includeAsync)
        _semaphore.Wait();
      if (_responses.Count > 0)
      {
        var exception = new LocalNoResponseException("Disposing while operations are pending", CreateDisposingException());
        foreach (var item in _responses)
        {
          item.Value.TrySetException(exception);
        }
        _responses.Clear();
      }
      _semaphore.Dispose();
      if (Target is not null)
      {
        TargetDeInitAuto();
        TargetDeInit(Target);
        TargetDisposeAuto();
      }
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
      try
      {
        if (_responses.Count > 0)
        {
          if (exception is not LocalNoResponseException)
          {
            exception = new LocalNoResponseException("Clearing while operations were pending", exception ?? new InvalidOperationException("Clearing responses"));
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
      }
      finally
      {
        _semaphore.Release();
      }
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
      try
      {
        if (_responses.Count > 0)
        {
          exception = new LocalNoResponseException("Exception occurred while operations were pending", exception);
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
      }
      finally
      {
        _semaphore.Release();
      }
    }

    /// <summary>
    /// Called on each incoming message<br/>
    /// First tries to check if the <paramref name="message"/> is a response and passes it along<br/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public override async Task OnMessageReceived(TPacket message, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);

      if ((message.PacketType & PacketType.Response)!=0)//Any response
      {
        try
        {
          await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch
        {
          return;
        }
        bool exists;
        TaskCompletionSource<object?>? response;
        try
        {
          exists = _responses.TryGetValue(message.Id, out response);
          if (exists)
            _responses.Remove(message.Id);
        }
        finally
        {
          _semaphore.Release();
        }
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
        if (message.PacketType == PacketType.CommandFailure)
        {
          try
          {
            throw (Exception)message.Parameter!;
          }
          catch (Exception ex)
          {
            try
            {
              await WriteAsync(PacketFactory.CreateResponse(message), default).ConfigureAwait(false);
            }
            catch (Exception e)
            {
              throw new AggregateException("Failed to respond for Failure Command",e, ex);
            }
          }
        }
        else if (message.PacketType == PacketType.CommandCancellation)
        {
          try
          {
            if (message.Parameter is Exception ex)
              throw ex;
            else
              throw new OperationCanceledException("Command Cancellation Received");
          }
          catch (Exception ex)
          {
            try
            {
              await WriteAsync(PacketFactory.CreateResponse(message), default).ConfigureAwait(false);
            }
            catch (Exception e)
            {
              throw new AggregateException("Failed to respond for Cancel Command", e, ex);
            }
          }
        }
        else
        {
          if (Functions.TryGetValue(message.Command, out var function))
          {
            await OnCommandFunction(message, function, cts.Token).ConfigureAwait(false);// The error handling of Target === sending response by the OnCommandFunction, else it throws on this side
          }
          else
          {
            OnUnknownMessage(message);
          }
        }
      }
      else
        OnUnknownMessage(message);
    }

    /// <summary>
    /// Filtered <see cref="OnMessageReceived(TPacket?, CancellationToken)"/> to only fire on commands in <see cref="Functions"/><br/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="function"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task OnCommandFunction(TPacket command, CommandFunction function, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);
      try
      {
        cts.Token.ThrowIfCancellationRequested();
        await function.Invoke((TCallback)this, command, cts.Token).ConfigureAwait(false);//If all worked fine it will send response on it's own
      }
      catch (OperationCanceledException ex)
      {
        try
        {
          if (cts.Token.IsCancellationRequested)//If parent token is cancelled it should be a RemoteNoResponseException to signify Pipe error
          {
            if(cancellationToken.IsCancellationRequested)
              await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Operation cancelled", ex)), default).ConfigureAwait(false);
            else if(LifetimeCancellation.IsCancellationRequested)
            {
              var exception = new RemoteNoResponseException("Operation performed on disposed object", CreateDisposedException(ex));
              await WriteAsync(PacketFactory.CreateResponseFailure(command, exception), default).ConfigureAwait(false);
            }
            else
              await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Operation timed out", new TimeoutException("Timed out",ex))), default).ConfigureAwait(false);
          }
          else
            await WriteAsync(PacketFactory.CreateResponseCancellation(command, ex), default).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          throw new AggregateException("Failed to send cancellation response", e, ex);
        }
      }
      catch (Exception ex)
      {
        try
        {
          await WriteAsync(PacketFactory.CreateResponseFailure(command, ex), default).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          throw new AggregateException("Failed to send failure response", e, ex);
        }
      }
    }

    /// <summary>
    /// Does nothing on each <paramref name="message"/> sent
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessageSent(TPacket message) { }

#nullable disable

    /// <summary>
    /// Awaits Response using <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    /// <typeparam name="T">requested result type</typeparam>
    /// <param name="command">packet with command</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> GetResponseAsync<T>(TPacket command, CancellationToken cancellationToken = default)
    {
      var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

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
        state.tcs.TrySetException(new LocalNoResponseException("Operation timed out", new TimeoutException("Timed out")));
      }, (tcs, cancellationToken, LifetimeCancellation)).ConfigureAwait(false);
#else
      using var ctr = cts.Token.Register(static x =>
        {
          var state = ((TaskCompletionSource<object> tcs, CancellationToken cancellationToken, CancellationToken lifetimeCancellation))x;
          if (state.tcs.Task.IsCompleted || state.cancellationToken.IsCancellationRequested || state.lifetimeCancellation.IsCancellationRequested)
            return;
          state.tcs.TrySetException(new LocalNoResponseException("Operation timed out",new TimeoutException("Timed out")));
        }, (tcs, cancellationToken, LifetimeCancellation));
#endif

      try
      {
        if(tcs.Task.IsCompleted)
          return CastOrDefault<T>(await tcs.Task.ConfigureAwait(false));

        try
        {
          await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
          try
          {
            _responses.Add(command.Id, tcs);
          }
          finally
          {
            _semaphore.Release();
          }
        }
        catch (Exception ex)
        {
          tcs.TrySetException(new LocalNoResponseException($"Queueing response failed", ex));
          return CastOrDefault<T>(await tcs.Task.ConfigureAwait(false));
        }

        if (tcs.Task.IsCompleted)
          return CastOrDefault<T>(await tcs.Task.ConfigureAwait(false));

        try
        {
          await WriteAsync(command, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          tcs.TrySetException(new LocalNoResponseException("Sending command failed", ex));
          return CastOrDefault<T>(await tcs.Task.ConfigureAwait(false));
        }

        return CastOrDefault<T>(await tcs.Task.ConfigureAwait(false));
      }
      catch (Exception e)
      {
        if (!tcs.Task.IsCompleted)//If the response was not set on the task...
          tcs.TrySetException(new LocalNoResponseException("Catch block reached, trying setting exception", e));
        throw;
      }
      finally
      {
        if (!tcs.Task.IsCompleted)
          tcs.TrySetException(new LocalNoResponseException("Finally block reached, trying setting exception", new InvalidOperationException("Failed to finish gracefully")));

        try
        {
          await _semaphore.WaitAsync(LifetimeCancellation).ConfigureAwait(false);
          try
          {
            _responses.Remove(command.Id);
          }
          finally
          {
            _semaphore.Release();
          }
        }
        catch
        {
          //Ignored
        }
      }
    }

#nullable restore

    /// <inheritdoc/>
    public override async Task WriteAsync(TPacket message, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);
      await Connection.WriteAsync(message, cts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public Task SendResponseAsync(TPacket message, CancellationToken cancellationToken = default)
    {
      var response = PacketFactory.CreateResponse(message);
      return WriteAsync(response, cancellationToken);
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket, object?)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    public Task SendResponseAsync<T>(TPacket message, T parameter, CancellationToken cancellationToken = default)
    {
      var response = PacketFactory.CreateResponse(message, parameter);
      return WriteAsync(response, cancellationToken);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TOut> RemoteAsync<TOut>(string callerName, CancellationToken cancellationToken = default)
    {
      var cmd = PacketFactory.CreateCommand(callerName);
      return GetResponseAsync<TOut>(cmd, cancellationToken);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TOut> RemoteAsync<TIn, TOut>(string callerName, TIn? parameter, CancellationToken cancellationToken = default)
    {
      var cmd = PacketFactory.CreateCommand(callerName, parameter);
      return GetResponseAsync<TOut>(cmd, cancellationToken);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoteAsync(string callerName, CancellationToken cancellationToken = default)
    {
      var cmd = PacketFactory.CreateCommand(callerName);
      return GetResponseAsync<object?>(cmd, cancellationToken);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoteAsync<TIn>(string callerName, TIn? parameter, CancellationToken cancellationToken = default)
    {
      var cmd = PacketFactory.CreateCommand(callerName, parameter);
      return GetResponseAsync<object?>(cmd, cancellationToken);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public TOut Remote<TOut>(string callerName)
    {
      try
      {
        return RemoteAsync<TOut>(callerName).Result;
      }
      catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)
      {
        throw ae.InnerException!;
      }
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public TOut Remote<TIn, TOut>(string callerName, TIn? parameter)
    {
      try
      {
        return RemoteAsync<TIn, TOut>(callerName, parameter).Result;
      }
      catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)
      {
        throw ae.InnerException!;
      }
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    public void Remote(string callerName)
    {
      try
      {
        RemoteAsync(callerName).Wait();
      }
      catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)
      {
        throw ae.InnerException!;
      }
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      try
      {
        RemoteAsync(callerName, parameter).Wait();
      }
      catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)
      {
        throw ae.InnerException!;
      }
    }
  }
}
