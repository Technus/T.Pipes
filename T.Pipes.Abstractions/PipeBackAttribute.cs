using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Parameter)]
  public class PipeBackAttribute : Attribute
  {
  }
}
