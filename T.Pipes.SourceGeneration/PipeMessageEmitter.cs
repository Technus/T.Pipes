using CodegenCS;
using System;
using System.Collections.Generic;
using System.Text;

namespace T.Pipes.SourceGeneration
{
  internal static class PipeMessageEmitter
  {
    public static void EmitPipeMessage(this ICodegenTextWriter writer, string[] parameters) => writer.WriteLine($$"""
      [Serializable]
      public struct PipeMessage<>
      {
        private PipeMessage(Guid command, Guid id)
        {
          Command = command;
          Id = id;
        }

        public PipeMessage(Guid command) : this(command, Guid.NewGuid())
        {
        }

        public Guid Id { get; private set; }
        public Guid Command { get; private set; }
      }
      """);
  }
}
