using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal class Parser
  {
    private Compilation compilation;
    private Action<Diagnostic> reportDiagnostic;
    private CancellationToken cancellationToken;

    internal Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
    {
      this.compilation = compilation;
      this.reportDiagnostic = reportDiagnostic;
      this.cancellationToken = cancellationToken;
    }

    internal TypeDefiniton GenerateType(TypeDeclarationSyntax classy)
    {
      return new(classy, GetHintName(classy));
    }

    private string GetHintName(TypeDeclarationSyntax classy)
    {
      var sb = new StringBuilder(128);
      sb.Append(classy.Identifier.ToString());

      while (TryGetParentSyntax<TypeDeclarationSyntax>(classy, out var cl))
      {
        sb.Insert(0, '.');
        sb.Insert(0, cl.Identifier.ToString());
      }
      if (TryGetParentSyntax<NamespaceDeclarationSyntax>(classy, out var ns))
      {
        sb.Insert(0, '.');
        sb.Insert(0, ns.Name.ToString());
      }
      return sb.ToString();
    }


    public static bool TryGetParentSyntax<T>(SyntaxNode? syntaxNode, [NotNullWhen(true)] out T? result) where T : SyntaxNode
    {
      // set defaults

      if (syntaxNode == null)
      {
        result = null;
        return false;
      }

      try
      {
        syntaxNode = syntaxNode.Parent;

        if (syntaxNode == null)
        {
          result = null;
          return false;
        }

        if (syntaxNode.GetType() == typeof(T))
        {
          var t = syntaxNode as T;
          if(t is not null)
          {
            result = t;
            return true;
          }
          result = null;
          return false;
        }

        return TryGetParentSyntax<T>(syntaxNode, out result);
      }
      catch
      {
        result = null;
        return false;
      }
    }
  }
}