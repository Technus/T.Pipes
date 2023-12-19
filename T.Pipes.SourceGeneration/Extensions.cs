using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace T.Pipes.SourceGeneration
{
  internal static class Extensions
  {
    internal static IMethodSymbol InvokeSymbol(this IEventSymbol eventSymbol) 
      => (IMethodSymbol)eventSymbol.Type.GetMembers().Where(x => x.Name == "Invoke").First();

    internal static string TypeUse(this ITypeSymbol symbol) 
      => symbol.ToDisplayString();

    internal static string Prefix(this IParameterSymbol parameterSymbol) => parameterSymbol.RefKind switch
    {
      RefKind.Ref => "ref ",
      RefKind.Out => "out ",
      RefKind.In or RefKind.In + 1 => "in ",
      _ => "",
    };

    internal static SemanticModel GetSemanticModel(this Compilation compilation, SyntaxNode syntaxNode) => compilation.GetSemanticModel(syntaxNode.SyntaxTree);

    internal static bool TryGetParentSyntax<T>(this SyntaxNode? syntaxNode, [NotNullWhen(true)] out T? result) where T : SyntaxNode
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

        if (syntaxNode is T requestedNode)
        {
          result = requestedNode;
          return true;
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
