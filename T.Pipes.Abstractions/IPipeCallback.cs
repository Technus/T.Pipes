using System;

namespace T.Pipes.Abstractions
{
  public interface IPipeCallback<TMessage> : IDisposable, IAsyncDisposable
  {
    void OnMessageSent(TMessage? message);
    void OnExceptionOccurred(Exception e);
    void OnMessageReceived(TMessage? message);
    void Connected(string connection);
    void Disconnected(string connection);
  }
}
