using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
  public class PipeMeAttribute : Attribute
  {
  }
}
