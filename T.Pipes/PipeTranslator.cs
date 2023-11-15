using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T.Pipes.Abstractions;

namespace T.Pipes
{
  public class PipeTranslator
  {

  }

  public partial class A
  {
    [PipeMe]
    partial void Kek();

  }
}
