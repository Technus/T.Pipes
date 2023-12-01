using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes
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
  }
}
