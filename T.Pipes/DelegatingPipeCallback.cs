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
    : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeClientCallback<TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    protected DelegatingPipeClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }
  }

  /// <summary>
  /// Callback and proxy implementation of <typeparamref name="TTargetAndCallback"/> for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TTargetAndCallback">target to operate on and the callback at the same time</typeparam>
  public abstract class DelegatingPipeServerCallback<TTargetAndCallback>
    : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, TTargetAndCallback, TTargetAndCallback>
    where TTargetAndCallback : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, PipeMessage, PipeMessageFactory, TTargetAndCallback, TTargetAndCallback>, IDisposable
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    protected DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
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
    : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, TTarget, TCallback>
    where TTarget : IDisposable
    where TCallback : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    protected DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  /// <inheritdoc/>
  public abstract class DelegatingPipeCallback<TPipe, TTarget, TCallback>
    : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
    where TTarget : IDisposable
    where TPipe : H.Pipes.IPipeConnection<PipeMessage>
    where TCallback : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget, TCallback>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    protected DelegatingPipeCallback(TPipe pipe) : base(pipe, new())
    {
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    protected DelegatingPipeCallback(TPipe pipe, TTarget target) : base(pipe, new(), target)
    {
    }
  }

  /// <summary>
  /// Base callback implementation for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/><br/>
  /// <see cref="DelegatingPipeClient{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TPipe"><see cref="H.Pipes.IPipeConnection{TPacket}"/></typeparam>
  /// <typeparam name="TPacket"><see cref="IPipeMessage"/></typeparam>
  /// <typeparam name="TPacketFactory"><see cref="IPipeMessageFactory{TPacket}"/></typeparam>
  /// <typeparam name="TTarget">target to operate on</typeparam>
  /// <typeparam name="TCallback">the final implementing type</typeparam>
  public abstract class DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
    : PipeCallbackBase<TPipe, TPacket, TPacketFactory, TCallback>, IPipeDelegatingCallback<TPacket>
    where TTarget : IDisposable
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget, TCallback>
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
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <param name="responseTimeoutMs">response timeout in ms</param>
    /// <exception cref="InvalidOperationException">when the this is not a valid <typeparamref name="TTarget"/> or null</exception>
    protected DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory, int responseTimeoutMs = Timeout.Infinite) : base(packetFactory)
    {
      ResponseTimeoutMs = responseTimeoutMs;
      Pipe = pipe;
      if (this is TTarget tt)
      {
        Target = tt;
      }

      if (_target is null)
      {
        throw new InvalidCastException($"In fact this is not {typeof(TTarget).FullName}");
      }
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    /// <param name="responseTimeoutMs">response timeout in ms</param>
    /// <exception cref="InvalidOperationException">when <paramref name="target"/> is null</exception>
    protected DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory, TTarget target, int responseTimeoutMs = Timeout.Infinite) : base(packetFactory)
    {
      ResponseTimeoutMs = responseTimeoutMs;
      Pipe = pipe;
      if (target is not null)
      {
        Target = target;
      }

      if (_target is null)
      {
        throw new InvalidCastException($"In fact target is not {typeof(TTarget).FullName}");
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

        _target = value;

        if (Target is not null)
        {
          TargetInitAuto();
          TargetInit(Target);
        }
        else
        {
          throw new InvalidCastException($"In fact target is not {typeof(TTarget).FullName}");
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

    /// <summary>
    /// Used to access data tunnel
    /// </summary>
    public TPipe Pipe { get; }

    /// <inheritdoc/>
    public int ResponseTimeoutMs { get; set; }

    /// <inheritdoc/>
    public override void OnConnected(string connection) => ClearResponsesWithCancelling();

    /// <inheritdoc/>
    public override void OnDisconnected(string connection) => ClearResponsesWithCancelling();

    /// <summary>
    /// Disposes own resources, not the <see cref="Pipe"/> nor the <see cref="Target"/>
    /// </summary>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
      await base.DisposeAsyncCore(disposing).ConfigureAwait(false);
      await _semaphore.WaitAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="Pipe"/> nor the <see cref="Target"/>
    /// </summary>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      if(includeAsync)
        _semaphore.Wait();
      if (_responses.Count > 0)
      {
        string name = Pipe switch
        {
          H.Pipes.IPipeServer<TPacket> server => $"Server Pipe: {server.PipeName}",
          H.Pipes.IPipeClient<TPacket> client => $"Server: {client.ServerName}, Pipe: {client.PipeName}",
          _ => "Unknown Pipe",
        };

        var disposingException = new ObjectDisposedException(name);

        foreach (var item in _responses)
        {
          item.Value.TrySetException(disposingException);
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
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetException(Exception)"/>
    /// </summary>
    /// <param name="e">exception to pass along</param>
    public virtual void ClearResponsesWithFailing(Exception e)
    {
      _semaphore.Wait();
      if(_responses .Count > 0)
      {
        foreach (var item in _responses)
        {
          item.Value.TrySetException(e);
        }
      }
      _responses.Clear();
      _semaphore.Release();
    }

    /// <summary>
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetCanceled()"/>
    /// </summary>
    public virtual void ClearResponsesWithCancelling()
    {
      _semaphore.Wait();
      if (_responses.Count > 0)
      {
        foreach (var item in _responses)
        {
          item.Value.TrySetCanceled();
        }
      }
      _responses.Clear();
      _semaphore.Release();
    }

    /// <inheritdoc/>
    public override void OnExceptionOccurred(Exception e) => ClearResponsesWithFailing(e);

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
        _semaphore.Wait();
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
          finally
          {
            WriteAsync(PacketFactory.CreateResponse(message)).Wait();
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
          finally
          {
            WriteAsync(PacketFactory.CreateResponse(message)).Wait();
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
    /// Else calls <see cref="OnUnknownMessage(TPacket)"/>
    /// </summary>
    /// <param name="command">packet containing command</param>
    protected virtual void OnCommandReceived(TPacket command)
    {
      if (Functions.TryGetValue(command.Command, out var function))
      {
        try
        {
          OnCommandFunction(command, function).Wait();
        }
        catch (AggregateException ae) when (ae.InnerExceptions.Count == 1)
        {
          throw ae.InnerException!;
        }
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
      try
      {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
        if (ResponseTimeoutMs == 0)
          cts.Cancel();
        else if (ResponseTimeoutMs > 0)
          cts.CancelAfter(ResponseTimeoutMs);
        await function.Invoke((TCallback)this, command, cts.Token).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await WriteAsync(PacketFactory.CreateResponseFailure(command, e)).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Handler for unhandled commands, should always throw to indicate unknown command
    /// </summary>
    /// <param name="invalidMessage">the packet in question</param>
    /// <exception cref="ArgumentException">always</exception>
    protected virtual void OnUnknownMessage(TPacket invalidMessage)
    {
      var message = Pipe switch
      {
        H.Pipes.IPipeServer<TPacket> server => $"Message unknown: {invalidMessage}, Server Pipe: {server.PipeName}",
        H.Pipes.IPipeClient<TPacket> client => $"Message unknown: {invalidMessage}, Server: {client.ServerName}, Pipe: {client.PipeName}",
        _ => $"Message unknown: {invalidMessage}, Unknown Pipe",
      };
      throw new ArgumentException(message, nameof(invalidMessage));
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
      CancellationTokenRegistration ctr = default;

      try
      {
#if NET5_0_OR_GREATER
        ctr = cts.Token.UnsafeRegister(static (x,ct) => ((TaskCompletionSource<object>)x!).TrySetCanceled(ct), tcs);
#else
        ctr = cts.Token.Register(static x =>
        {
          var (tcs, ct) = ((TaskCompletionSource<object>, CancellationToken))x;
          tcs.TrySetCanceled(ct);
        }, (tcs, cts.Token));
#endif

        await _semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
        _responses.Add(command.Id, tcs);
        _semaphore.Release();
        await WriteAsync(command, cts.Token).ConfigureAwait(false);
        try
        {
          return (T)await tcs.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
          throw new NoResponseException(command.Command, ex);
        }
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          tcs.TrySetException(ex);
          throw;
        }
        else
        {
          var e = new TimeoutException("Timeout expired", ex);
          tcs.TrySetException(e);
          throw e;
        }
      }
      catch (Exception e)
      {
        tcs.TrySetException(e);
        throw;
      }
      finally
      {
        ctr.Dispose();
        if (!tcs.Task.IsCompleted)
          tcs.TrySetException(new InvalidOperationException("Failed to finish gracefully."));
        await _semaphore.WaitAsync().ConfigureAwait(false);
        _responses.Remove(command.Id);
        _semaphore.Release();
      }
    }

#nullable restore

    /// <summary>
    /// Writes to the pipe directly and calls the Callback On Write
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>do not write to the pipe directly, use that instead, (or the Wrapping Client/Server)</remarks>
    public async Task WriteAsync(TPacket message, CancellationToken cancellationToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, LifetimeCancellation);
      if (ResponseTimeoutMs == 0)
        cts.Cancel();
      else if (ResponseTimeoutMs > 0)
        cts.CancelAfter(ResponseTimeoutMs);
      await Pipe.WriteAsync(message, cts.Token).ConfigureAwait(false);
      OnMessageSent(message);
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
