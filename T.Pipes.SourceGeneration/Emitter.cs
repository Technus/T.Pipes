using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal class Emitter
  {
    private CancellationToken cancellationToken;

    public Emitter(CancellationToken cancellationToken) => this.cancellationToken = cancellationToken;

    internal (string HintName, string Source) EmitType(TypeDefiniton classDefinition) => (classDefinition.Name, "namespace Egg { class Nog {} }");
  }
}
