namespace T.Pipes.Abstractions
{
  public interface IPipeMessageFactory<T> where T : IPipeMessage
  {
    T Create(string command);
    T Create(string command, object? parameter);
    T CreateResponse(T commandMessage);
    T CreateResponse(T commandMessage, object? parameter);
  }
}
