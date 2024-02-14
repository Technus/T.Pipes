using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <summary>
  /// Base Class for Callbacks
  /// </summary>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TPacketFactory"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class PipeCallbackBase<TPacket, TPacketFactory, TCallback>
    : PipeCallbackBase<TPacket, TCallback>
    where TPacket : IPipeMessage
    where TPacketFactory : IPipeMessageFactory<TPacket>
    where TCallback : PipeCallbackBase<TPacket, TPacketFactory, TCallback>
  {
    internal static T CastOrDefault<T>(object? o) => o is T t ? t : default!;

    /// <summary>
    /// Packet factory being used
    /// </summary>
    protected internal TPacketFactory PacketFactory { get; internal set; }

    /// <summary>
    /// The base constructor
    /// </summary>
    /// <param name="packetFactory"></param>
    protected PipeCallbackBase(TPacketFactory packetFactory) 
      => PacketFactory = packetFactory;
  }

  /// <summary>
  /// Base Class for Callbacks
  /// </summary>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TCallback"></typeparam>
  public abstract class PipeCallbackBase<TPacket, TCallback>
    : UncheckedBaseClass, IPipeCallback<TPacket>
    where TPacket : IPipeMessage
    where TCallback : PipeCallbackBase<TPacket, TCallback>
  {
    private IPipeConnection<TPacket>? _connection;

    /// <summary>
    /// Used to access data tunnel should be set immediately after calling ctor
    /// </summary>
    public IPipeConnection<TPacket> Connection
    {
      get => _connection!;
      set
      {
        if (_connection is null)
          _connection = value ?? throw new InvalidOperationException("Trying to set to null");
        else
          throw new InvalidOperationException("Value already set");
      }
    }

    /// <summary>
    /// Writes to the pipe
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>do not write directly, use that instead, as a common point to add timeout and stuff</remarks>
    public abstract Task WriteAsync(TPacket message, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual void OnStarting()
    {

    }

    /// <inheritdoc/>
    public virtual void OnStarted()
    {

    }

    /// <inheritdoc/>
    public virtual void OnStopping()
    {

    }

    /// <inheritdoc/>
    public virtual void OnStopped()
    {

    }

    /// <inheritdoc/>
    public abstract void OnConnected(string connection);

    /// <inheritdoc/>
    public abstract void OnDisconnected(string connection);

    /// <inheritdoc/>
    public abstract void OnExceptionOccurred(Exception exception);

    /// <inheritdoc/>
    public abstract Task OnMessageReceived(TPacket message, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract void OnMessageSent(TPacket message);

    /// <summary>
    /// Handler for unhandled commands, should always throw to indicate unknown command
    /// </summary>
    /// <param name="invalidMessage">the packet in question</param>
    /// <exception cref="ArgumentException">always</exception>
    protected internal virtual void OnUnknownMessage(TPacket invalidMessage)
    {
      var message = $"Message unknown: {invalidMessage}, ServerName: {Connection.ServerName}, PipeName: {Connection.PipeName}";
      throw new ArgumentException(message, nameof(invalidMessage));
    }

    /// <summary>
    /// Helper to create disposed exception
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ObjectDisposedException CreateDisposingException() 
      => new($"ServerName: {Connection.ServerName}, PipeName: {Connection.PipeName}");

    /// <summary>
    /// Helper to create disposed exception
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ObjectDisposedException CreateDisposedException(Exception exception) 
      => new($"ServerName: {Connection.ServerName}, PipeName: {Connection.PipeName}", exception);

    /// <summary>
    /// Disposes <see cref="Connection"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected override async ValueTask DisposeAsyncCore(bool disposing) 
      => await Connection.DisposeAsync().ConfigureAwait(false);

    /// <summary>
    /// Disposes <see cref="Connection"/>
    /// </summary>
    /// <param name="disposing"></param>
    /// <param name="includeAsync"></param>
    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      if (includeAsync)
      {
        Connection.Dispose();
      }
    }
  }
}
