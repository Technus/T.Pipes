using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Test.Abstractions
{
  public static class PipeConstants
  {
    public const string ServerPipeName = "T.Pipes.Test";
    public const string ClientDisplayName = "T.Pipes.Test.Client";
    public const string ServerDisplayName = "T.Pipes.Test.Server";
    public const string ClientExeName = "T.Pipes.Test.Client.exe";
    public const int ConnectionAwaitTimeMs = 10000;

    public const string Create = "T.Pipes.Create";
  }
}
