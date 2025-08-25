using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace T.Pipes.SourceGeneration
{
  internal class Parser
  {
    private static DiagnosticDescriptor MissingParametersDescriptor { get; } = new("T_Pipes_MissingParameters", "Missing Parameters", "Missing Parameters", "Attributes", DiagnosticSeverity.Error, true);
    private static DiagnosticDescriptor InvalidParametersDescriptor { get; } = new("T_Pipes_InvalidParameters", "Invalid Parameters", "Invalid Parameters", "Attributes", DiagnosticSeverity.Error, true);
    private static DiagnosticDescriptor NoSymbolDescriptor { get; } = new("T_Pipes_NoSymbol", "No Symbol", "No Symbol", "Attributes", DiagnosticSeverity.Error, true);

    private readonly Compilation compilation;
    private readonly Action<Diagnostic> reportDiagnostic;
    private CancellationToken cancellationToken;

    internal Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
    {
      this.compilation = compilation;
      this.reportDiagnostic = reportDiagnostic;
      this.cancellationToken = cancellationToken;
    }

    internal TypeDefinition GenerateType(TypeDeclarationSyntax classy)
    {
      var (served, used, implementing) = GetMembers(classy);
      return new(
        typeDeclarationSyntax: classy,
        name: GetHintName(classy),
        namespaceName: classy.TryGetParentSyntax<NamespaceDeclarationSyntax>(out var parent) 
          ? parent.Name.ToString() 
          : classy.TryGetParentSyntax<FileScopedNamespaceDeclarationSyntax>(out var parentF)
          ? parentF.Name.ToString()
          : throw new ArgumentException("Has no Namespace", nameof(classy)),
        typeList: GetTypeList(classy),
        serveMemberDeclarations: served,
        usedMemberDeclarations: used,
        implementingTypes: implementing
      );
    }

    private (List<ISymbol> served, List<ISymbol> used, List<ITypeSymbol> implementing) GetMembers(TypeDeclarationSyntax classy)
    {
      var served = new List<ISymbol>();
      var used = new List<ISymbol>();
      var implementing = new List<ITypeSymbol>();

      foreach (var item in classy.Members)
      {
        foreach (var attributeListSyntax in item.AttributeLists)
        {
          foreach (var attributeSyntax in attributeListSyntax.Attributes)
          {
            var attributeSymbol = compilation.GetSemanticModel(attributeSyntax).GetSymbolInfo(attributeSyntax).Symbol;
            if (attributeSymbol == null)
            {
              // weird, we couldn't get the symbol, ignore it
              continue;
            }

            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();

            if (fullName == SourceGenerator.PipeServeAttribute)
            {
              // return the parent class of the method
              served.Add(MakeMember(item));
            }
            if (fullName == SourceGenerator.PipeUseAttribute)
            {
              // return the parent class of the method
              used.Add(MakeMember(item));
            }
          }
        }
      }

      foreach (var attributeListSyntax in classy.AttributeLists)
      {
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
          var attributeSymbol = compilation.GetSemanticModel(attributeSyntax).GetSymbolInfo(attributeSyntax).Symbol;
          if (attributeSymbol == null)
          {
            // weird, we couldn't get the symbol, ignore it
            continue;
          }

          var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
          var fullName = attributeContainingTypeSymbol.ToDisplayString();

          if (fullName == SourceGenerator.PipeServeAttribute)
          {
            // return the parent class of the method
            served.AddRange(MakeMembers(attributeSyntax));
            implementing.AddRange(MakeImplementedTypes(attributeSyntax));
          }
          if (fullName == SourceGenerator.PipeUseAttribute)
          {
            // return the parent class of the method
            used.AddRange(MakeMembers(attributeSyntax));
          }
        }
      }

      return (served, used, implementing);
    }

    private ISymbol MakeMember(MemberDeclarationSyntax member)
    {
      var semantic = compilation.GetSemanticModel(member);
      var typeDef = semantic.GetSymbolInfo(member).Symbol;
      if(typeDef is null)
      {
        reportDiagnostic(Diagnostic.Create(NoSymbolDescriptor,member.GetLocation()));
        throw new InvalidOperationException();
      }
      return typeDef;
    }

    private IEnumerable<ITypeSymbol> MakeImplementedTypes(AttributeSyntax attribute)
    {
      if (attribute.ArgumentList is null)
      {
        reportDiagnostic(Diagnostic.Create(MissingParametersDescriptor, attribute.GetLocation()));
        yield break;
      }

      foreach (var item in attribute.ArgumentList.Arguments)
      {
        switch (item.Expression.Kind())
        {
          //TODO SyntaxKind.StringLiteralExpression
          case SyntaxKind.TypeOfExpression:
            {
              var type = ((TypeOfExpressionSyntax)item.Expression).Type;
              var semantic = compilation.GetSemanticModel(type);
              if (semantic.GetSymbolInfo(type).Symbol is INamedTypeSymbol typeDef)
              {
                yield return typeDef;
              }
              break;
            }
          default:
            {
              reportDiagnostic(Diagnostic.Create(InvalidParametersDescriptor, attribute.GetLocation()));
              yield break;
            }
        }
      }

    }

    private IEnumerable<ISymbol> MakeMembers(AttributeSyntax attribute)
    {
      if(attribute.ArgumentList is null)
      {
        reportDiagnostic(Diagnostic.Create(MissingParametersDescriptor,attribute.GetLocation()));
        yield break;
      }

      foreach (var item in attribute.ArgumentList.Arguments)
      {
        switch (item.Expression.Kind())
        {
          //TODO SyntaxKind.StringLiteralExpression
          case SyntaxKind.TypeOfExpression:
            {
              var type = ((TypeOfExpressionSyntax)item.Expression).Type;
              var semantic = compilation.GetSemanticModel(type);
              if (semantic.GetSymbolInfo(type).Symbol is INamedTypeSymbol typeDef)
              {

                foreach (var member in typeDef.GetMembers())
                {
                  yield return member;
                }
              }
              break;
            }
          default:
            {
              reportDiagnostic(Diagnostic.Create(InvalidParametersDescriptor, attribute.GetLocation()));
              yield break;
            }
        }
      }

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
      else if(classy.TryGetParentSyntax<FileScopedNamespaceDeclarationSyntax>(out var fns))
      {
          sb.Insert(0, '.');
          sb.Insert(0, fns.Name.ToString());
      }
      return sb.ToString();
    }
  }
}