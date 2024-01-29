using System;
using System.Diagnostics;
using System.Xml.Linq;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal sealed class ServerCallback : SpawningPipeCallback
  {
    private readonly SurrogateProcessWrapper _process;

    public ServerCallback(string pipeName) : base(PipeConstants.ResponseTimeMs) 
      => _process = new SurrogateProcessWrapper(new ProcessStartInfo(PipeConstants.ClientExeName, pipeName));

    protected override IPipeDelegatingConnection<PipeMessage> CreateProxy(string command, string pipeName) => command switch
    {
      PipeConstants.Create => new DelegatingServerAuto(pipeName),
      PipeConstants.CreateInvalid => throw new InvalidOperationException("Should not be reached, as it never gets response to do so! (when Lazy creation is enabled client side [it is by default])"),
      _ => throw new ArgumentException($"Invalid {nameof(command)}: {command}", nameof(command)),
    };

    public Task<DelegatingServerAuto> CreateAsync() => RequestProxyAsync<DelegatingServerAuto>(PipeConstants.Create);
    public Task<DelegatingServerAuto> CreateInvalidAsync() => RequestProxyAsync<DelegatingServerAuto>(PipeConstants.CreateInvalid);

    public IAbstract? CreateOrDefault() => RequestProxyOrDefault<DelegatingServerAuto>(PipeConstants.Create)?.Callback?.AsIAbstract;
    public IAbstract? CreateInvalidOrDefault() => RequestProxyOrDefault<DelegatingServerAuto>(PipeConstants.CreateInvalid)?.Callback?.AsIAbstract;

    /// <summary>
    /// One way to create safe getters is to wrap the <see cref="SpawningPipeCallback.RequestProxyAsync{T}(string, CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    public IAbstract? CreateOrDefaultUserCode()
    {
      DelegatingServerAuto? disposable = null;
      try
      {
        disposable = CreateAsync().Result;//Call of function to wrap
        return disposable.Callback.AsIAbstract;
      }
      catch (Exception ex)
      {
        if (ex is AggregateException ae && ae.InnerExceptions.Count == 1)
          ex = ae.InnerException!;

        ex.PrintNicely();
        disposable?.Dispose();
        return default;
      }
    }

    /// <summary>
    /// One way to create safe getters is to wrap the <see cref="SpawningPipeCallback.RequestProxyAsync{T}(string, CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    public IAbstract? CreateInvalidOrDefaultUserCode()
    {
      DelegatingServerAuto? disposable = null;
      try
      {
        disposable = CreateInvalidAsync().Result;//Call of function to wrap
        return disposable.Callback.AsIAbstract;
      }
      catch (Exception ex)
      {
        if (ex is AggregateException ae && ae.InnerExceptions.Count == 1)
          ex = ae.InnerException!;

        ex.PrintNicely();
        disposable?.Dispose();
        return default;
      }
    }

    public override Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.Cyan);
      return base.OnMessageReceived(message, cancellationToken);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.Cyan);
      base.OnMessageSent(message);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      ("E: " + exception.ToString()).WriteLine(ConsoleColor.Cyan);
      base.OnExceptionOccurred(exception);
    }

    public override void OnStarting()
    {
      ("R: "+ Connection.ServerName).WriteLine(ConsoleColor.Cyan);
      base.OnStarting();
      _process.StartProcess().Wait();
    }

    public override void OnStopping()
    {
      ("S: " + Connection.ServerName).WriteLine(ConsoleColor.Cyan);
      base.OnStopping();
      _process.StopProcess().Wait();
    }

    protected override void DisposeCore(bool disposing, bool includeAsync)
    {
      base.DisposeCore(disposing, includeAsync);
      _process.Dispose();
    }
  }

  /// <summary>
  /// Main server used to control Delegating Server Instances
  /// </summary>
  internal sealed class Server : SpawningPipeServer<ServerCallback>
  {
    public Server(string name = PipeConstants.ServerPipeName)
      : this(new H.Pipes.PipeServer<PipeMessage>(name, formatter: new Formatter()))
    {
    }

    private Server(H.Pipes.PipeServer<PipeMessage> pipe) : base(pipe, new(pipe.PipeName))
    {
    }
  }
}