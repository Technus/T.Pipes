using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal readonly struct TypeDefiniton
  {
    public TypeDeclarationSyntax TypeDeclarationSyntax { get; }
    public string Name { get; }

    public TypeDefiniton(TypeDeclarationSyntax typeDeclarationSyntax, string name)
    {
      TypeDeclarationSyntax = typeDeclarationSyntax;
      Name = name;
    }
  }
}
