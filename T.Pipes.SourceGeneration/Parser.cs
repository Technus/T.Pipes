using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal class Parser
  {
    private readonly Compilation compilation;
    private readonly Action<Diagnostic> reportDiagnostic;
    private CancellationToken cancellationToken;

    internal Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
    {
      this.compilation = compilation;
      this.reportDiagnostic = reportDiagnostic;
      this.cancellationToken = cancellationToken;
    }

    internal TypeDefinition GenerateType(TypeDeclarationSyntax classy) => new()
    {
      TypeDeclarationSyntax = classy,
      Name = GetHintName(classy),
      Namespace = classy.TryGetParentSyntax<NamespaceDeclarationSyntax>(out var parent) ? parent.Name.ToString() : throw new ArgumentException("Has no Namespace", nameof(classy)),
      TypeList = GetTypeList(classy),
      UsingList = new List<string> { },
      MemberDeclarations = GetMembers(classy),
    };

    private static List<MemberDeclarationSyntax> GetMembers(TypeDeclarationSyntax classy)
    {
      var members = new List<MemberDeclarationSyntax>();

      foreach (var item in classy.Members)
      {

      }

      return members;
    }

    private static List<string> GetTypeList(TypeDeclarationSyntax classy)
    {
      var list = new List<string>
      {
        GetTypeDeclaration(classy)
      };

      while (classy.TryGetParentSyntax<TypeDeclarationSyntax>(out var cl))
      {
        classy = cl;
        list.Insert(0, GetTypeDeclaration(classy));
      }

      return list;
    }

    private static string GetTypeDeclaration(TypeDeclarationSyntax syntax)
    {
      var typeType = syntax switch
      {
        StructDeclarationSyntax _ => "struct",
        ClassDeclarationSyntax _ => "class",
        RecordDeclarationSyntax _ => "record",
        _ => throw new ArgumentException("Invalid Type declaration", nameof(syntax)),
      };
      return $"{syntax.Modifiers} {typeType} {syntax.Identifier}{syntax.TypeParameterList?.ToString() ?? string.Empty}";
    }

    private static string GetHintName(TypeDeclarationSyntax classy)
    {
      var sb = new StringBuilder(128);
      sb.Append(classy.Identifier.ToString());

      while (classy.TryGetParentSyntax<TypeDeclarationSyntax>(out var cl))
      {
        classy = cl;
        sb.Insert(0, '.');
        sb.Insert(0, classy.Identifier.ToString());
      }
      if (classy.TryGetParentSyntax<NamespaceDeclarationSyntax>(out var ns))
      {
        sb.Insert(0, '.');
        sb.Insert(0, ns.Name.ToString());
      }
      return sb.ToString();
    }

  }
}