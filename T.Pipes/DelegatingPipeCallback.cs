using System;
using System.Collections.Generic;
using System.Linq;
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
    /// 
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    public DelegatingPipeClientCallback(H.Pipes.PipeClient<PipeMessage> pipe, TTarget target) : base(pipe, target)
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
    /// 
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    public DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
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
    /// 
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    public DelegatingPipeServerCallback(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe)
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
    public DelegatingPipeCallback(TPipe pipe) : base(pipe, PipeMessageFactory.Instance)
    {
    }

    /// <summary>
    /// Creates the callback, must be done with the same pipe as in the pipe connection holding it.
    /// </summary>
    /// <param name="pipe">the same pipe as in the pipe connection holding it</param>
    /// <param name="target">the actual implementation of <typeparamref name="TTarget"/></param>
    public DelegatingPipeCallback(TPipe pipe, TTarget target) : base(pipe, PipeMessageFactory.Instance, target)
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
    : IPipeDelegatingCallback<TPacket>
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
    /// <returns>return value/s from target</returns>
    public delegate void CommandFunction(TCallback callback, TPacket message);

    private readonly TaskCompletionSource<object?> _connectedOnce = new();
    private readonly TaskCompletionSource<object?> _failedOnce = new();
    private readonly Dictionary<Guid, TaskCompletionSource<object?>> _responses = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
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
          throw new InvalidOperationException($"In fact target is not {typeof(TTarget).FullName}");
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

    /// <summary>
    /// Used to create packets
    /// </summary>
    public TPacketFactory PacketFactory { get; }

    /// <summary>
    /// Use to check if connection was established correctly the first time
    /// </summary>
    public Task ConnectedOnce => _connectedOnce.Task;

    /// <summary>
    /// Use to check if connection was failed at least once
    /// </summary>
    public Task FailedOnce => _failedOnce.Task;

    /// <inheritdoc/>
    public int ResponseTimeoutMs { get; set; } = -1;

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
    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Disposes own resources, not the <see cref="Pipe"/> nor the <see cref="Target"/>
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask DisposeAsync()
    {
      _semaphore.Dispose();
      if (_responses.Any())
      {
        var name = Pipe is H.Pipes.IPipeServer<TPacket> server ? $"Server Pipe: {server.PipeName}"
          : Pipe is H.Pipes.IPipeClient<TPacket> client ? $"Server: {client.ServerName}, Pipe: {client.PipeName}"
          : "Unknown Pipe";
        var disposingException = new ObjectDisposedException(name);

        foreach (var item in _responses)
        {
          _ = item.Value.TrySetException(disposingException);
        }
        _responses.Clear();
      }
      if (_target is not null)
      {
        TargetDeInitAuto();
        TargetDeInit(_target);
      }
      _ = _failedOnce.TrySetCanceled();
      _ = _connectedOnce.TrySetCanceled();
      return default;
    }

    /// <summary>
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetException(Exception)"/>
    /// </summary>
    /// <param name="e">exception to pass along</param>
    public virtual void Clear(Exception e)
    {
      _semaphore.Wait();
      foreach (var item in _responses)
      {
        _ = item.Value.TrySetException(e);
      }
      _responses.Clear();
      _ = _semaphore.Release();
    }

    /// <summary>
    /// Clears the response awaiting tasks by <see cref="TaskCompletionSource{TResult}.TrySetCanceled()"/>
    /// </summary>
    public virtual void Clear()
    {
      _semaphore.Wait();
      foreach (var item in _responses)
      {
        _ = item.Value.TrySetCanceled();
      }
      _responses.Clear();
      _ = _semaphore.Release();
    }

    /// <inheritdoc/>
    public virtual void OnExceptionOccurred(Exception e)
    {
      Clear(e);
      _ = _failedOnce.TrySetException(e);
      _ = _connectedOnce.TrySetException(e);
    }

    /// <summary>
    /// Called on each incoming message<br/>
    /// First tries to check if the <paramref name="message"/> is a response and passes it along<br/>
    /// Else calls <see cref="OnCommandReceived(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    public virtual void OnMessageReceived(TPacket? message)
    {
      if (message is null)
      {
        return;
      }

      if (_responses.TryGetValue(message.Id, out var response))
      {
        _semaphore.Wait();
        _ = _responses.Remove(message.Id);
        _ = _semaphore.Release();
        _ = response.TrySetResult(message.Parameter);
      }
      else
      {
        OnCommandReceived(message);
      }
    }

    /// <summary>
    /// Filtered <see cref="OnMessageReceived(TPacket?)"/> to only fire on commands and not responses<br/>
    /// First tries to check if the <paramref name="command"/> is a function command <br/>
    /// Else calls <see cref="OnUnknownCommand(TPacket)"/>
    /// </summary>
    /// <param name="command">packet containing command</param>
    protected virtual void OnCommandReceived(TPacket command)
    {
      if (Functions.TryGetValue(command.Command, out var function))
      {
        function.Invoke((TCallback)this, command);
      }
      else
      {
        OnUnknownCommand(command);
      }
    }

    /// <summary>
    /// Handler for unhandled commands, should always throw to indicate unknown command
    /// </summary>
    /// <param name="invalidMessage">the packet in question</param>
    /// <exception cref="ArgumentException">always</exception>
    protected virtual void OnUnknownCommand(TPacket invalidMessage)
      => throw new ArgumentException($"Message unknown: {invalidMessage}", nameof(invalidMessage));

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
      var tcs = new TaskCompletionSource<object>();
      _semaphore.Wait();
      _responses.Add(command.Id, tcs);
      _semaphore.Release();
      var responseTask = tcs.Task;
      if (ResponseTimeoutMs >= 0)
      {
        using var cts = new CancellationTokenSource();
        if (Task.WhenAny(responseTask, Task.Delay(ResponseTimeoutMs, cts.Token)).Result == responseTask)
        {
          cts.Cancel();
        }
        else
        {
          _semaphore.Wait();
          _responses.Remove(command.Id);
          _semaphore.Release();
          tcs.TrySetException(new TimeoutException());
        }
      }

      return (T)responseTask.Result;
    }

    /// <summary>
    /// Awaits Response using <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    /// <typeparam name="T">requested result type</typeparam>
    /// <param name="command">packet with command</param>
    /// <returns></returns>
    public async Task<T> GetResponseAsync<T>(TPacket command)
    {
      var tcs = new TaskCompletionSource<T>();
      await _semaphore.WaitAsync();
      _responses.Add(command.Id, tcs);
      _semaphore.Release();
      var responseTask = tcs.Task;
      if (ResponseTimeoutMs >= 0)
      {
        using var cts = new CancellationTokenSource();
        if (await Task.WhenAny(responseTask, Task.Delay(ResponseTimeoutMs, cts.Token)) == responseTask)
        {
          cts.Cancel();
        }
        else
        {
          await _semaphore.WaitAsync();
          _responses.Remove(command.Id);
          _semaphore.Release();
          tcs.TrySetException(new TimeoutException());
        }
      }
      return (T)await responseTask;
    }

#nullable restore

    /// <summary>
    /// Writes to the pipe directly and calls the Callback On Write
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <remarks>do not write to the pipe directly, use that instead, (or the Wrapping Client/Server)</remarks>
    public void Write(TPacket message)
    {
      OnMessageSent(message);
      _ = Pipe.WriteAsync(message);
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket)"/>
    /// </summary>
    /// <param name="message"></param>
    public void SendResponse(TPacket message)
    {
      var response = PacketFactory.CreateResponse(message);
      Write(response);
    }

    /// <summary>
    /// Sends response using <see cref="IPipeMessageFactory{TPacket}.CreateResponse(TPacket, object?)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="parameter"></param>
    public void SendResponse<T>(TPacket message, T parameter)
    {
      var response = PacketFactory.CreateResponse(message, parameter);
      Write(response);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public async Task<TOut> RemoteAsync<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Write(cmd);
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
      var cmd = PacketFactory.Create(callerName, parameter);
      Write(cmd);
      return await GetResponseAsync<TOut>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public async Task RemoteAsync(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Write(cmd);
      await GetResponseAsync<object?>(cmd);
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
      var cmd = PacketFactory.Create(callerName, parameter);
      Write(cmd);
      await GetResponseAsync<object?>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting result
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <returns></returns>
    public TOut Remote<TOut>(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Write(cmd);
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
      var cmd = PacketFactory.Create(callerName, parameter);
      Write(cmd);
      return GetResponse<TOut>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <param name="callerName">the command or function name</param>
    public void Remote(string callerName)
    {
      var cmd = PacketFactory.Create(callerName);
      Write(cmd);
      GetResponse<object?>(cmd);
    }

    /// <summary>
    /// Delegates the work to the other side, by sending command and awaiting response
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="callerName">the command or function name</param>
    /// <param name="parameter"></param>
    public void Remote<TIn>(string callerName, TIn? parameter)
    {
      var cmd = PacketFactory.Create(callerName, parameter);
      Write(cmd);
      GetResponse<object?>(cmd);
    }
  }
}
