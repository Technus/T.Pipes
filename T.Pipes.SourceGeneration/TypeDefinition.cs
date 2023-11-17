using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal struct TypeDefinition
  {
    public TypeDeclarationSyntax TypeDeclarationSyntax { get; set; }

    /// <summary>
    /// The output file name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The namespace
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// The usings to write
    /// </summary>
    public List<string> UsingList { get; set; }

    /// <summary>
    /// In case of types rooted in other types
    /// </summary>
    public List<string> TypeList { get; set; }
  }
}
