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
  /// <typeparam name="TTarget">The target type implementing <typeparamref name="TTarget"/></typeparam>
  public class DelegatingPipeClientCallback<TTarget>
    : DelegatingPipeCallback<H.Pipes.PipeClient<PipeMessage>, TTarget>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    public DelegatingPipeClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
    {
    }
  }

  /// <summary>
  /// Callback and proxy implementation of <typeparamref name="TTarget"/> for:<br/>
  /// <see cref="DelegatingPipeServer{TPipe, TPacket, TPacketFactory, TTarget, TCallback}"/>
  /// </summary>
  /// <typeparam name="TTarget"></typeparam>
  public class DelegatingPipeServerCallback<TTarget>
    : DelegatingPipeCallback<H.Pipes.PipeServer<PipeMessage>, TTarget>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    public DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  /// <inheritdoc/>
  public class DelegatingPipeCallback<TPipe, TTarget>
    : DelegatingPipeCallback<TPipe, PipeMessage, PipeMessageFactory, TTarget>
    where TPipe : H.Pipes.IPipeConnection<PipeMessage>
  {
    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    public DelegatingPipeCallback(TPipe pipe) : base(pipe, new())
    {
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    public DelegatingPipeCallback(TPipe pipe, TTarget target) : base(pipe, new(), target)
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
  /// <typeparam name="TTarget"></typeparam>
  public class DelegatingPipeCallback<TPipe, TPacket, TPacketFactory, TTarget>
    : IPipeCallback<TPacket>
    where TPipe : H.Pipes.IPipeConnection<TPacket>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
  {
    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    private readonly TaskCompletionSource<object?> _failedOnce = new();
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _responses = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, Func<object?, object?>> _functions = [];
    private TTarget _target;

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <exception cref="InvalidOperationException">when the this is not a valid <typeparamref name="TTarget"/> or null</exception>
    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
      if (this is TTarget tt)
      {
        Target = tt;
      }

      if (_target is null)
      {
        throw new InvalidOperationException($"In fact this is not {typeof(TTarget).FullName}");
      }
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="packetFactory">to create <typeparamref name="TPacket"/></param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    /// <exception cref="InvalidOperationException">when <paramref name="target"/> is null</exception>
    public DelegatingPipeCallback(TPipe pipe, TPacketFactory packetFactory, TTarget target)
    {
      Pipe = pipe;
      PacketFactory = packetFactory;
      if (target is not null)
      {
        Target = target;
      }

      if (_target is null)
      {
        throw new InvalidOperationException($"In fact target is not {typeof(TTarget).FullName}");
      }
    }

    /// <summary>
    /// For client an instance of <typeparamref name="TTarget"/>
    /// For server returns 'this'
    /// </summary>
    public TTarget Target
    {
      get => _target;
      private set
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
    /// <param name="target">the target to deinitialize against</param>
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
    /// Used to acces data tunnel
    /// </summary>
    protected TPipe Pipe { get; }

    /// <summary>
    /// Used to create packets
    /// </summary>
    protected TPacketFactory PacketFactory { get; }

    /// <summary>
    /// Use to check if connection was established correctly the first time
    /// </summary>
    public Task ConnectedOnce => _connectedOnce.Task;

    /// <summary>
    /// Use to check if connection was failed at least once
    /// </summary>
    public Task FailedOnce => _failedOnce.Task;

    /// <inheritdoc/>
    public virtual void Connected(string connection)
    {
      Clear();
      _ = _connectedOnce.TrySetResult(null);
    }

    /// <inheritdoc/>
    public virtual void Disconnected(string connection)
    {
      Clear();
      _ = _failedOnce.TrySetResult(null);
      _ = _connectedOnce.TrySetCanceled();
    }

    /// <summary>
    /// <see cref="DisposeAsync"/>
    /// </summary>
    public void Dispose()
    {
      DisposeAsync().AsTask().Wait();
    }

    /// <summary>
    /// Disposes own resources, not the <see cref="Pipe"/> nor the <see cref="Target"/>
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask DisposeAsync()
    {
      await _semaphore.WaitAsync();
      foreach (KeyValuePair<Guid, TaskCompletionSource<object?>> item in _responses)
      {
        _ = item.Value.TrySetCanceled();
      }
      _responses.Clear();
      if (_target is not null)
      {
        TargetDeInitAuto();
        TargetDeInit(_target);
      }
      _functions.Clear();
      _ = _failedOnce.TrySetCanceled();
      _ = _connectedOnce.TrySetCanceled();
      _semaphore.Dispose();
    }

    /// <summary>
    /// Clears the response awaiters by <see cref="TaskCompletionSource{TResult}.TrySetCanceled()"/>
    /// </summary>
    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (KeyValuePair<Guid, TaskCompletionSource<object?>> item in _responses)
      {
        _ = item.Value.TrySetCanceled();
      }
      _responses.Clear();
      _ = _semaphore.Release();
    }

    /// <inheritdoc/>
    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear();
      _ = _failedOnce.TrySetResult(null);
      _ = _connectedOnce.TrySetException(e);
    }

    /// <summary>
    /// Called on each incoming message<br/>
    /// First tries to check if the <paramref name="message"/> is a response and passes it along<br/>
    /// Else calls <see cref="OnCommandReceived(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    public void OnMessageReceived(TPacket? message)
    {
      if (message is null)
      {
        return;
      }

      if (_responses.TryGetValue(message.Id, out TaskCompletionSource<object?>? response))
      {
        _ = response.TrySetResult(message.Parameter);
        _semaphore.Wait();
        _ = _responses.Remove(message.Id);
        _ = _semaphore.Release();
      }
      else
      {
        OnCommandReceived(message);
      }
    }

    /// <summary>
    /// Filtered <see cref="OnMessageReceived(TPacket?)"/> to only fire on commands and not responses<br/>
    /// First tries to check if the <paramref name="command"/> is a function command <br/>
    /// Else calls <see cref="OnCommandReceivedAuto(TPacket)"/><br/>
    /// Else call <see cref="OnUnknownCommand(TPacket)"/>
    /// </summary>
    /// <param name="command">packet containing command</param>
    protected virtual void OnCommandReceived(TPacket command)
    {
      if (_functions.TryGetValue(command.Command, out Func<object?, object?>? function))
      {
        _ = Pipe.WriteAsync(PacketFactory.CreateResponse(command, function.Invoke(command.Parameter)));
      }
      else if (OnCommandReceivedAuto(command))
      {
        return;
      }
      OnUnknownCommand(command);
    }

    /// <summary>
    /// Stub for source generator, to write command handling
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual bool OnCommandReceivedAuto(TPacket message)
    {
      return false;
    }

    /// <summary>
    /// Handler for unhandled commands, should always throw to indicate unknown command
    /// </summary>
    /// <param name="invalidMessage">the packet in question</param>
    /// <exception cref="ArgumentException">always</exception>
    protected virtual void OnUnknownCommand(TPacket invalidMessage)
    {
      throw new ArgumentException($"Message is unknown: {invalidMessage}", nameof(invalidMessage));
    }
    
    /// <summary>
    /// Does nothing on each <paramref name="message"/> sent
    /// </summary>
    /// <param name="message"></param>
    public virtual void OnMessageSent(TPacket? message) { }

#nullable disable

    /// <summary>
    /// Awaits Response using <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    /// <typeparam name="T">requested result type</typeparam>
    /// <param name="command">packet with command</param>
    /// <returns></returns>
    public T GetResponse<T>(TPacket command)
    {
      TaskCompletionSource<object> tcs = new();
      _semaphore.Wait();
      _responses.Add(command.Id, tcs);
      _ = _semaphore.Release();
      return (T)tcs.Task.Result;
    }

    /// <summary>
    /// Awaits Response using <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    /// <typeparam name="T">requested result type</typeparam>
    /// <param name="command">packet with command</param>
    /// <returns></returns>
    public async Task<T> GetResponseAsync<T>(TPacket command)
    {
      TaskCompletionSource<object> tcs = new();
      await _semaphore.WaitAsync();
      _responses.Add(command.Id, tcs);
      _ = _semaphore.Release();
      return (T)await tcs.Task;
    }

#nullable restore

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    public void SendResponse(TPacket message)
    {
      TPacket response = PacketFactory.CreateResponse(message);
      Pipe.WriteAsync(response).Wait();
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendResponseAsync(TPacket message)
    {
      TPacket response = PacketFactory.CreateResponse(message);
      await Pipe.WriteAsync(response);
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket, object?)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="parameter"></param>
    public void SendResponse<T>(TPacket message, T parameter)
    {
      TPacket response = PacketFactory.CreateResponse(message, parameter);
      Pipe.WriteAsync(response).Wait();
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket, object?)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public async Task SendResponseAsync<T>(TPacket message, T parameter)
    {
      TPacket response = PacketFactory.CreateResponse(message, parameter);
      await Pipe.WriteAsync(response);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public async Task<TOut> RemoteAsync<TOut>(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public async Task<TOut> RemoteAsync<TIn, TOut>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public async Task RemoteAsync(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      await Pipe.WriteAsync(cmd);
      _ = await GetResponseAsync<object?>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public async Task RemoteAsync<TIn>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      await Pipe.WriteAsync(cmd);
      _ = await GetResponseAsync<object?>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public TOut Remote<TOut>(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
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
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      return GetResponse<TOut>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    public void Remote(string callerName)
    {
      TPacket cmd = PacketFactory.Create(callerName);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      TPacket cmd = PacketFactory.Create(callerName, parameter);
      Pipe.WriteAsync(cmd).Wait();
      _ = GetResponse<object?>(cmd);
    }

    /// <summary>
    /// Adds a function call instead of command call
    /// </summary>
    /// <param name="callerName"></param>
    /// <param name="function"></param>
    public void SetFunction(string callerName, Func<object?, object?> function)
    {
      _functions[callerName] = function;
    }

    /// <summary>
    /// Removes a function call reverting to command call
    /// </summary>
    /// <param name="callerName"></param>
    public void RemoveFunction(string callerName)
    {
      _ = _functions.Remove(callerName);
    }
  }
}
