using System;
using System.Runtime.Serialization;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Thrown when command was sent but there was no response
  /// </summary>
  [Serializable]
  public class NoResponseException : Exception
  {
    /// <summary>
    /// Constructs and exception for when command was sent but there was no response
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public NoResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Serialization ctor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
#if NET8_0_OR_GREATER
    [Obsolete("This is being still used for remoting kindof...", DiagnosticId = "SYSLIB0051")]
#endif
    protected NoResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
