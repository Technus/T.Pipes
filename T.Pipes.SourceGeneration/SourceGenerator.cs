using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace T.Pipes.SourceGeneration
{
  [Generator(LanguageNames.CSharp)]
  public class SourceGenerator : IIncrementalGenerator
  {
    const string PipeMeAttribute = "T.Pipes.Abstractions.PipeMeAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext ctx)
    {
      var classDeclarations = ctx.SyntaxProvider.CreateSyntaxProvider(
        predicate: static (s, _) => HasAttributes(s),
        transform: static (ctx, _) => GetClass(ctx))
        .Where(static m => m is not null);

      var compilationAndClasses = ctx.CompilationProvider.Combine(classDeclarations.Collect());

      ctx.RegisterSourceOutput(compilationAndClasses, static (spc, src) => Execute(src.Left, src.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
      if (classes.IsDefaultOrEmpty)
      {
        // nothing to do yet
        return;
      }

      classes = classes.Where(static x => x is not null).Distinct().ToImmutableArray();
      if(classes.Length > 0)
      {
        var p = new Parser(compilation, context.ReportDiagnostic, context.CancellationToken);
        var e = new Emitter(context.CancellationToken);

        foreach (var item in classes)
        {
          var classDefinition = p.GenerateClass(item!);
          var outputDefinition = e.EmitClass(classDefinition);
          context.AddSource(outputDefinition.HintName + ".g.cs", SourceText.From(outputDefinition.Source, Encoding.UTF8));
        }
      }
    }

    private static bool HasAttributes(SyntaxNode node) => node is MemberDeclarationSyntax memberDeclaration && memberDeclaration.AttributeLists.Count > 0;

    private static ClassDeclarationSyntax? GetClass(GeneratorSyntaxContext context)
    {
      // we know the node is a MethodDeclarationSyntax thanks to IsSyntaxTargetForGeneration
      var memberDeclarationSyntax = (MemberDeclarationSyntax)context.Node;

      // loop through all the attributes on the method
      foreach (var attributeListSyntax in memberDeclarationSyntax.AttributeLists)
      {
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
          var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
          if (attributeSymbol == null)
          {
            // weird, we couldn't get the symbol, ignore it
            continue;
          }

          var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
          var fullName = attributeContainingTypeSymbol.ToDisplayString();

          // Is the attribute the [PipeMe] attribute?
          if (fullName == PipeMeAttribute)
          {
            // return the parent class of the method
            return memberDeclarationSyntax.Parent as ClassDeclarationSyntax;
          }
        }
      }

      // we didn't find the attribute we were looking for
      return null;
    }
  }
}
