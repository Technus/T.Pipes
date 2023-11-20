﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace T.Pipes.SourceGeneration
{
  internal static class Extensions
  {
    public static SemanticModel GetSemanticModel(this Compilation compilation, SyntaxNode syntaxNode) => compilation.GetSemanticModel(syntaxNode.SyntaxTree);

    public static bool TryGetParentSyntax<T>(this SyntaxNode? syntaxNode, [NotNullWhen(true)] out T? result) where T : SyntaxNode
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