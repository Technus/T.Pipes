using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  /// <summary>
  /// Use to decorate Client side Interface host holding the actual implementation
  /// </summary>
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = true)]
  public class PipeUseAttribute : Attribute
  {
    /// <summary>
    /// Use to decorate Client side Interface host holding the actual implementation
    /// </summary>
    /// <param name="type">type to implement should be an interface without generic members</param>
    public PipeUseAttribute(Type type) => Type = type;

    /// <summary>
    /// type to write delegation to
    /// </summary>
    public Type Type { get; }
  }
}
