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
    where TTarget : IDisposable
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
    where TTargetAndCallback : DelegatingPipeCallback<PipeMessage, PipeMessageFactory, TTargetAndCallback, TTargetAndCallback>, IDisposable
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
    where TTarget : IDisposable
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
    where TTarget : IDisposable
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
    where TTarget : IDisposable
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
      if (this is TTarget tt)
      {
        Target = tt;
      }

      if (_target is null)
      {
        throw new InvalidCastException($"This is not {typeof(TTarget).FullName}");
      }
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
      {
        Target = target;
      }

      if (_target is null)
      {
        throw new InvalidOperationException($"Target is not {typeof(TTarget).FullName}");
      }
    }

    /// <summary>
    /// Static Collection of Command functions
    /// </summary>
    public static Dictionary<string, CommandFunction> Functions { get; } = [];

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
        if (_responses.Count > 0)
        {
          var exception = new LocalNoResponseException("Target changed", new InvalidOperationException("Changing Target while operations were pending"));
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

    IDisposable IPipeDelegatingCallback<TPacket>.Target => Target;

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
      if (_responses.Count > 0)
      {
        var exception = new LocalNoResponseException("Connected", new InvalidOperationException("Connection occured while operations were pending"));
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
        var exception = new LocalNoResponseException("Disconnected", new InvalidOperationException("Disconnection occured while operations were pending"));
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
        var exception = new LocalNoResponseException("Disposing", CreateDisposingException());
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
      }
    }

    /// <summary>
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetCanceled()"/>
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
    /// Called on each incoming message<br/>
    /// First tries to check if the <paramref name="message"/> is a response and passes it along<br/>
    /// Else calls <see cref="OnCommandReceived(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessageReceived(TPacket message)
    {
      if ((message.PacketType & PacketType.Response)!=0)//Any response
      {
        try
        {
          _semaphore.Wait(LifetimeCancellation);
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
              response!.TrySetCanceled();
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
              WriteAsync(PacketFactory.CreateResponse(message)).Wait();
            }
            catch (Exception e)
            {
              throw new AggregateException(e,ex);
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
              WriteAsync(PacketFactory.CreateResponse(message)).Wait();
            }
            catch (Exception e)
            {
              throw new AggregateException(e, ex);
            }
          }
        }
        else
          OnCommandReceived(message);
      }
      else
        OnUnknownMessage(message);
    }

    /// <summary>
    /// Filtered <see cref="OnMessageReceived(TPacket?)"/> to only fire on commands and not responses<br/>
    /// First tries to check if the <paramref name="command"/> is a function command and calls <see cref="OnCommandFunction(TPacket, CommandFunction, CancellationToken)"/><br/>
    /// Else calls <see cref="PipeCallbackBase{TPacket, TCallback}.OnUnknownMessage(TPacket)"/>
    /// </summary>
    /// <param name="command">packet containing command</param>
    protected virtual void OnCommandReceived(TPacket command)
    {
      if (Functions.TryGetValue(command.Command, out var function))
      {
        Task.Run(() => OnCommandFunction(command, function));// The error handling of Target === sending response by the OnCommandFunction, else it throws on this side
      }
      else
      {
        OnUnknownMessage(command);
      }
    }

    /// <summary>
    /// Filtered <see cref="OnMessageReceived(TPacket?)"/> to only fire on commands in <see cref="Functions"/><br/>
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
              await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Cancelled", ex)), default).ConfigureAwait(false);
            else if(LifetimeCancellation.IsCancellationRequested)
            {
              var exception = new RemoteNoResponseException("Disposed", CreateDisposedException(ex));
              await WriteAsync(PacketFactory.CreateResponseFailure(command, exception), default).ConfigureAwait(false);
            }
            else
              await WriteAsync(PacketFactory.CreateResponseFailure(command, new RemoteNoResponseException("Cancelled", new TimeoutException("Timed out",ex))), default).ConfigureAwait(false);
          }
          else
            await WriteAsync(PacketFactory.CreateResponseCancellation(command, ex), default).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          throw new AggregateException(e,ex);
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
          throw new AggregateException(e, ex);
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

      try
      {
        if(tcs.Task.IsCompleted)
          return (T)await tcs.Task.ConfigureAwait(false);

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
          tcs.TrySetException(new LocalNoResponseException("Queueing TaskCompletionSource", ex));
          return (T)await tcs.Task.ConfigureAwait(false);
        }

        if (tcs.Task.IsCompleted)
          return (T)await tcs.Task.ConfigureAwait(false);

        try
        {
          await WriteAsync(command, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          tcs.TrySetException(new LocalNoResponseException("Sending Command", ex));
          return (T)await tcs.Task.ConfigureAwait(false);
        }

        return (T)await tcs.Task.ConfigureAwait(false);
      }
      finally
      {
        if (!tcs.Task.IsCompleted)
          tcs.TrySetException(new LocalNoResponseException("Finally block reached", new InvalidOperationException("Failed to finish gracefully.")));

        try
        {
          await _semaphore.WaitAsync(LifetimeCancellation).ConfigureAwait(false);
          _responses.Remove(command.Id);
          _semaphore.Release();
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
