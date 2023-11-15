using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal readonly struct ClassDefinition
  {
    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
    public string Name { get; }

    public ClassDefinition(ClassDeclarationSyntax classDeclarationSyntax, string name)
    {
      ClassDeclarationSyntax = classDeclarationSyntax;
      Name = name;
    }
  }
}
