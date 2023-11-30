using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace T.Pipes
{
  /// <summary>
  /// Some task helpers
  /// </summary>
  public static class TaskExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsCompletedSuccessfully(this Task task)
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
      => task.IsCompletedSuccessfully;
#else
      => task.Status == TaskStatus.RanToCompletion;
#endif
  }
}
