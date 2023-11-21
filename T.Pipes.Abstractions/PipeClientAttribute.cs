using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Event)]
  public class PipeClientAttribute : Attribute
  {
  }
}
