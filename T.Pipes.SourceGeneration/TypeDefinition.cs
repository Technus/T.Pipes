using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal class TypeDefinition
  {
    public TypeDefinition(
      TypeDeclarationSyntax typeDeclarationSyntax, 
      string name, 
      string namespaceName,
      List<string> typeList,
      List<ISymbol> serveMemberDeclarations,
      List<ISymbol> usedMemberDeclarations, 
      List<ITypeSymbol> implementingTypes)
    {
      TypeDeclarationSyntax = typeDeclarationSyntax;
      Name = name;
      Namespace = namespaceName;
      TypeList = typeList;
      ServeMemberDeclarations = serveMemberDeclarations;
      UsedMemberDeclarations = usedMemberDeclarations;
      Commands = [];
      ImplementingTypes = implementingTypes;
    }

    public TypeDeclarationSyntax TypeDeclarationSyntax { get; }

    /// <summary>
    /// The output file name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The namespace
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// In case of types rooted in other types
    /// </summary>
    public List<string> TypeList { get; }

    /// <summary>
    /// Members in the type to write
    /// </summary>
    public List<ISymbol> ServeMemberDeclarations { get; }

    /// <summary>
    /// Members in the type to write
    /// </summary>
    public List<ISymbol> UsedMemberDeclarations { get; }

    public Dictionary<string, (ISymbol method, IMethodSymbol invoke)> Commands { get; }

    public List<ITypeSymbol> ImplementingTypes { get; }
  }
}
