using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
  [AttributeUsage(AttributeTargets.Method|AttributeTargets.Constructor|AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Event)]
  public class PipeMeAttribute : Attribute
  {
  }
}
