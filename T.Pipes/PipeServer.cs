using H.Pipes.Args;
using System.Threading;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  /// <inheritdoc/>
  public class PipeServer<TCallback>
    : PipeServer<H.Pipes.PipeServer<PipeMessage>, PipeMessage, TCallback>
    where TCallback : IPipeCallback<PipeMessage>
  {
    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(H.Pipes.PipeServer<PipeMessage> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <inheritdoc/>
  public class PipeServer<TPacket, TCallback>
    : PipeServer<H.Pipes.PipeServer<TPacket>, TPacket, TCallback>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(string pipe, TCallback callback) : base(new(pipe), callback)
    {
    }

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(H.Pipes.PipeServer<TPacket> pipe, TCallback callback) : base(pipe, callback)
    {
    }
  }

  /// <summary>
  /// Base pipe server implementation
  /// </summary>
  /// <typeparam name="TPipe"><see cref="H.Pipes.IPipeServer{TPacket}"/></typeparam>
  /// <typeparam name="TPacket"></typeparam>
  /// <typeparam name="TCallback"><see cref="IPipeCallback{TPacket}"/></typeparam>
  public class PipeServer<TPipe, TPacket, TCallback>
    : PipeConnection<TPipe, TPacket, TCallback>
    where TPipe : H.Pipes.IPipeServer<TPacket>
    where TCallback : IPipeCallback<TPacket>
  {
    /// <summary>
    /// Checks if it is running
    /// </summary>
    public override bool IsRunning => Pipe.IsStarted;

    /// <summary>
    /// Same as <see cref="ServerName"/>
    /// </summary>
    public override string PipeName => Pipe.PipeName;

    /// <inheritdoc/>
    public override string ServerName => Pipe.PipeName;

    /// <summary>
    /// Creates the base pipe server implementation
    /// </summary>
    /// <param name="pipe">pipe to use</param>
    /// <param name="callback">callback to use</param>
    public PipeServer(TPipe pipe, TCallback callback) : base(pipe, callback)
    {
      Pipe.ClientDisconnected += OnClientDisconnected;
      Pipe.ClientConnected += OnClientConnected;
    }

    private void OnClientDisconnected(object? sender, ConnectionEventArgs<TPacket> e) 
      => Callback.Disconnected(e.Connection.PipeName);

    private void OnClientConnected(object? sender, ConnectionEventArgs<TPacket> e) 
      => Callback.Connected(e.Connection.PipeName);

    /// <inheritdoc/>
    public override Task StartAsync(CancellationToken cancellationToken = default)
      => Pipe.StartAsync(cancellationToken);

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken = default)
      => Pipe.StopAsync(cancellationToken);

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      Pipe.ClientDisconnected -= OnClientDisconnected;
      Pipe.ClientConnected -= OnClientConnected;
    }
  }
}
