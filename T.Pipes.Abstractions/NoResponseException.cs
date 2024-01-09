using System;
using System.Runtime.Serialization;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Thrown when command response was not delivered for any reason
  /// </summary>
  [Serializable]
  public sealed class RemoteNoResponseException : NoResponseException
  {
    /// <summary>
    /// Constructs and exception for when command was sent but there was no response
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public RemoteNoResponseException(string message, Exception innerException) : base(message, innerException)
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
#pragma warning disable CS0628 // New protected member declared in sealed type
    protected RemoteNoResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
#pragma warning restore CS0628 // New protected member declared in sealed type
  }

  /// <summary>
  /// Thrown when command response was not delivered for any reason
  /// </summary>
  public sealed class LocalNoResponseException : NoResponseException
  {
    /// <summary>
    /// Constructs and exception for when command was sent but there was no response
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public LocalNoResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }

  /// <summary>
  /// Thrown when command response was not delivered for any reason
  /// </summary>
  public abstract class NoResponseException : Exception
  {
    /// <summary>
    /// Constructs and exception for when command was sent but there was no response
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected NoResponseException(string message, Exception innerException) : base(message, innerException)
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
