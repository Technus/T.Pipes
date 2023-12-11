using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Use to decorate Server side Interface host so proxy implementation
  /// </summary>
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = true)]
  public class PipeServeAttribute : Attribute
  {
    /// <summary>
    /// Use to decorate Server side Interface host so proxy implementation
    /// </summary>
    /// <param name="type">type to implement should be an interface without generic members</param>
    public PipeServeAttribute(Type type) => Type = type;

    /// <summary>
    /// type to implement
    /// </summary>
    public Type Type { get; }
  }
}
