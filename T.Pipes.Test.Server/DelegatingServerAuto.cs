using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<short>))]
  internal sealed partial class DelegatingCallback : DelegatingPipeServerCallback<DelegatingCallback>
  {
    public DelegatingCallback() : base(PipeConstants.ResponseTimeMs)
    {
    }

#if TRACE
    public override Task OnMessageReceived(PipeMessage message, CancellationToken cancellationToken = default)
    {
      ("I: " + message.ToString()).WriteLine(ConsoleColor.DarkCyan);
      return base.OnMessageReceived(message, cancellationToken);
    }

    public override void OnMessageSent(PipeMessage message)
    {
      ("O: " + message.ToString()).WriteLine(ConsoleColor.DarkCyan);
      base.OnMessageSent(message);
    }

    public override void OnExceptionOccurred(Exception exception)
    {
      ("E: " + exception.ToString()).WriteLine(ConsoleColor.DarkCyan);
      base.OnExceptionOccurred(exception);
    }
#endif
  }

  internal sealed class DelegatingServerAuto : DelegatingPipeServer<DelegatingCallback>
  {
    public DelegatingServerAuto(string pipe) : base(pipe, new())
    {
    }
  }
}