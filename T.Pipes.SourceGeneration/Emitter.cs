﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CodegenCS;
using Microsoft.CodeAnalysis;

namespace T.Pipes.SourceGeneration
{
  internal class Emitter
  {
    private CancellationToken cancellationToken;
    private readonly ICodegenTextWriter writer;

    public Emitter(CancellationToken cancellationToken)
    {
      this.cancellationToken = cancellationToken;
      writer = new CodegenTextWriter();
      writer.IndentString = "  ";
    }

    internal (string hintName, string source) EmitType(TypeDefinition typeDefinition)
    {
      writer.WriteLine($$"""
      //Generated by T.Pipes.SourceGeneration

      {{() => typeDefinition.UsingList.ForEach(x => RenderUsing(x))}}
      namespace {{typeDefinition.Namespace}}
      {
      #nullable enable
      {{() => RenderContent(typeDefinition)}}
      #nullable restore
      }
      """);
      return (typeDefinition.Name + ".g.cs", writer.ToString());
    }

    private void RenderUsing(string usingName) => writer.WriteLine($$"""
      using {{usingName}};
      """);

    private void RenderContent(TypeDefinition typeDefinition) => writer.Write($$"""
      {{() => RenderTypeStarts(typeDefinition)}}
      {{() => RenderInnerContent(typeDefinition)}}
      {{() => typeDefinition.TypeList.ForEach(x => RenderTypeEnd())}}
      """);

    private void RenderTypeStarts(TypeDefinition typeDefinition)
    {
      for (int i = 0; i < typeDefinition.TypeList.Count; i++)
      {
        RenderTypeStart(typeDefinition, typeDefinition.TypeList[i], i == typeDefinition.TypeList.Count-1);
      }
    }

    private void RenderTypeStart(TypeDefinition typeDefinition, string name, bool isTarget) => writer
      .IncreaseIndent()
      .WriteLine($$"""
      {{name}}{{()=>RenderBaseTypes(typeDefinition.ImplementingTypes, isTarget)}}
      {
      """);

    private void RenderBaseTypes(IReadOnlyList<ISymbol> implementingTypes, bool isTarget)
    {
      if (implementingTypes.Count == 0)
        return;

      writer.Write(" : ");
      RenderStrings(implementingTypes.Select(x => x.ToDisplayString()).ToArray());
    }

    private void RenderTypeEnd() => writer
      .WriteLine($$"""
      }
      """)
      .DecreaseIndent();

    private void RenderInnerContent(TypeDefinition typeDefinition)
    {
      writer.IncreaseIndent();
      typeDefinition.ServeMemberDeclarations.ForEach(symbol => RenderSymbol(typeDefinition, symbol, true));
      typeDefinition.UsedMemberDeclarations.ForEach(symbol => RenderSymbol(typeDefinition, symbol, false));
      RenderStaticSelector(typeDefinition);
      RenderTargetHandling(typeDefinition);
      RenderImplementation(typeDefinition);
      typeDefinition.ImplementingTypes?.ForEach(RenderCast);
      writer.DecreaseIndent();
    }

    private void RenderCast(ISymbol symbol) => writer
      .WriteLine($$"""
      public {{symbol?.ToDisplayString()}} As{{() => RenderTypeName((INamedTypeSymbol)symbol!,true)}} => ({{symbol?.ToDisplayString()}})Target;
      """);

    private void RenderImplementation(TypeDefinition typeDefinition) 
      => typeDefinition.ServeMemberDeclarations.ForEach(RenderImplementation);

    private void RenderImplementation(ISymbol x)
    {
      switch (x)
      {
        case IPropertySymbol propertySymbol: RenderNewProperty(propertySymbol); break;
        case IEventSymbol eventSymbol: RenderNewEvent(eventSymbol); break;
        case IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.Ordinary: RenderNewMethod(methodSymbol); break;
        default: writer.Write("//").WriteLine(x.Name); break;
      }
    }

    private void RenderNewMethod(IMethodSymbol methodSymbol) => writer
      .WriteLine($$"""
      {{methodSymbol.ReturnType.ToDisplayString()}} {{methodSymbol.ContainingType.ToDisplayString()}}.{{methodSymbol.Name}}({{()=>RenderParameters(methodSymbol.Parameters)}})
        => {{()=>RenderName(methodSymbol,true)}}({{()=>RenderStrings(methodSymbol.Parameters.Select(x=> Prefix(x) + x.Name).ToArray())}});
      """);

    private string Prefix(IParameterSymbol parameterSymbol)
    {
      switch(parameterSymbol.RefKind)
      {
        case RefKind.Ref: return "ref ";
        case RefKind.Out: return "out ";
        case RefKind.In:
        case RefKind.In + 1: return "in ";
        default: return "";
      }
    }

    private void RenderNewEvent(IEventSymbol eventSymbol) => writer
      .WriteLine($$"""
      internal {{eventSymbol.Type.ToDisplayString()}} {{() => RenderName(eventSymbol, true,"event_")}};
      event {{eventSymbol.Type.ToDisplayString()}} {{eventSymbol.ContainingType.ToDisplayString()}}.{{eventSymbol.Name}}
      { 
        add => {{() => RenderName(eventSymbol, true, "event_")}} += value;
        remove => {{() => RenderName(eventSymbol, true, "event_")}} -= value;
      }

      """);

    private void RenderNewProperty(IPropertySymbol propertySymbol) => writer
      .WriteLine($$"""
      {{propertySymbol.Type.ToDisplayString()}} {{propertySymbol.ContainingType.ToDisplayString()}}.{{propertySymbol.Name}}
      { 
        get => {{()=>RenderName(propertySymbol,true,"get_")}}(); 
        set => {{() => RenderName(propertySymbol, true, "set_")}}(value); 
      }
      """);

    private void RenderTargetHandling(TypeDefinition typeDefinition) => writer
      .WriteLine($$"""

      [System.ComponentModel.Description("TargetInit")]
      protected override void TargetInitAuto()
      {
        base.TargetInitAuto();
      {{() => RenderEventHandling(typeDefinition.UsedMemberDeclarations.Where(x => x is IEventSymbol).Cast<IEventSymbol>().ToArray(), true)}}
      }
      
      [System.ComponentModel.Description("TargetDeInit")]
      protected override void TargetDeInitAuto()
      {
        base.TargetDeInitAuto();
      {{() => RenderEventHandling(typeDefinition.UsedMemberDeclarations.Where(x => x is IEventSymbol).Cast<IEventSymbol>().ToArray(), false)}}
      }

      """);

    private void RenderEventHandling(IReadOnlyList<IEventSymbol> symbols, bool v)
    {
      writer.IncreaseIndent();

      foreach (var item in symbols)
      {
        RenderEventHandling(item, v);
      }

      writer.DecreaseIndent();
    }

    private void RenderEventHandling(IEventSymbol x, bool v)
    {
      writer.Write($$"""(({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name}}""");
      writer.Write(v ? " += ": " -= ");
      RenderName(x, false, "invoke_");
      writer.WriteLine(';');
    }

    private void RenderStaticSelector(TypeDefinition typeDefinition) => writer
      .Write($$"""
      static {{typeDefinition.TypeDeclarationSyntax.Identifier}}(){
        RegisterCommandSelectorFunctions();
      }

      [System.ComponentModel.Description("CommandSelector")]
      static protected void RegisterCommandSelectorFunctions()
      {{() => RenderStaticSelectorBody(typeDefinition)}}
      """);

    private void RenderStaticSelectorBody(TypeDefinition typeDefinition)
    {
      writer.WriteLine('{');
      writer.IncreaseIndent();
      writer.WriteLine("#pragma warning disable CS8600, CS8604, CS8605");
      foreach(var item in typeDefinition.Commands)
      {
        if(item.Value.method is IMethodSymbol x)
        {
          var (input, output) = GetIO(x);
          writer.Write($$"""Functions["{{item.Key}}"] = static (callback, message) => { """);

          if (!item.Value.invoke.ReturnsVoid || output.Count > 0)
          {
            writer.Write("callback.SendResponse(message, ");
          }

          writer.Write("callback.").Write(item.Key).Write('(');
          if (input.Count > 0)
          {
            writer.Write('(');
            if (input.Count > 1)
              writer.Write('(');
            RenderStrings(input.Select(x => x.Type.ToDisplayString()).ToArray());
            if (input.Count > 1)
              writer.Write(')');
            writer.Write(")message.Parameter");
          }

          if (!item.Value.invoke.ReturnsVoid || output.Count > 0)
          {
            writer.Write(")");
          }

          writer.Write(");");

          if (item.Value.invoke.ReturnsVoid && output.Count == 0)
          {
            writer.Write(" callback.SendResponse(message);");
          }

          writer.WriteLine(" };");
        }
        else if(item.Value.method is IEventSymbol e)
        {
          writer.Write($$"""Functions["{{item.Key}}"] = static (callback, message) => { """);

          if (!item.Value.invoke.ReturnsVoid)
          {
            writer.Write("callback.SendResponse(message, ");
          }

          writer.Write("callback.").Write(item.Key).Write('(');
          if (item.Value.invoke.Parameters.Length > 0)
          {
            writer.Write('(');
            RenderStrings(item.Value.invoke.Parameters.Select(x => x.Type.ToDisplayString()).ToArray());
            writer.Write(")message.Parameter");
          }

          if (!item.Value.invoke.ReturnsVoid)
          {
            writer.Write(")");
          }

          writer.Write(");");

          if (item.Value.invoke.ReturnsVoid)
          {
            writer.Write(" callback.SendResponse(message);");
          }

          writer.WriteLine(" };");
        }
      }
      writer.WriteLine("#pragma warning restore CS8600, CS8604, CS8605");
      writer.DecreaseIndent();
      writer.WriteLine();
      writer.WriteLine('}');
    }

    private void RenderSelector(TypeDefinition typeDefinition) => writer
      .Write($$"""
      [System.ComponentModel.Description("CommandSelector")]
      protected override bool OnCommandReceivedAuto(T.Pipes.PipeMessage message)
      {{() => RenderSelectorBody(typeDefinition)}}
      """);

    private void RenderSelectorBody(TypeDefinition typeDefinition)
    {
      writer.WriteLine('{');
      writer.IncreaseIndent();
      writer.WriteLine($$"""
        var parameter = message.Parameter;
        #pragma warning disable CS8600, CS8604, CS8605
        switch(message.Command){
          {{() => RenderCases(typeDefinition)}}
          default: return base.OnCommandReceivedAuto(message);
        }
        #pragma warning restore CS8600, CS8604, CS8605
        """);
      writer.DecreaseIndent();
      writer.WriteLine();
      writer.WriteLine('}');
    }

    private void RenderCases(TypeDefinition typeDefinition)
    {
      foreach (var item in typeDefinition.Commands)
      {
        writer.Write("case \"");
        writer.Write(item.Key).Write("\": ");


        if (item.Value.method is IMethodSymbol x)
        {
          var (input, output) = GetIO(x);

          if (!item.Value.invoke.ReturnsVoid || output.Count>0)
          {
            writer.Write("SendResponse(message, ");
          }

          //if(!x.ReturnsVoid && output.Count == 0)
          //{
          //  writer.Write("_ = ");//todo
          //  writer.Write('(');
          //  writer.Write(x.ReturnType.ToDisplayString());
          //  writer.Write(')');
          //}
          //else if (x.ReturnsVoid && output.Count == 1)
          //{
          //  writer.Write("_ = ");//todo
          //  writer.Write('(');
          //  writer.Write(output[0].Type.ToDisplayString());
          //  writer.Write(')');
          //}
          //else if(!x.ReturnsVoid || output.Count > 0)
          //{
          //  writer.Write("_ = ");//todo
          //  writer.Write('(');
          //  writer.Write('(');
          //  if(!x.ReturnsVoid)
          //    writer.Write(x.ReturnType.ToDisplayString());
          //  if(!x.ReturnsVoid && output.Count>0)
          //    writer.Write(", ");
          //  RenderStrings(output.Select(x=>x.Type.ToDisplayString()).ToArray());
          //  writer.Write(')');
          //  writer.Write(')');
          //}

          writer.Write(item.Key).Write('(');
          if (input.Count > 0)
          {
            writer.Write('(');
            if (input.Count > 1)
              writer.Write('(');
            RenderStrings(input.Select(x=>x.Type.ToDisplayString()).ToArray());
            if (input.Count > 1)
              writer.Write(')');
            writer.Write(")parameter");
          }

          if (!item.Value.invoke.ReturnsVoid || output.Count>0)
          {
            writer.Write(")");
          }

          writer.Write(");");

          if (item.Value.invoke.ReturnsVoid && output.Count ==0)
          {
            writer.Write(" SendResponse(message);");
          }
        }
        else if (item.Value.method is IEventSymbol e)
        {
          if (!item.Value.invoke.ReturnsVoid)
          {
            writer.Write("SendResponse(message, ");
          }

          //if (!item.Value.invoke.ReturnsVoid)
          //{
          //  writer.Write("_ = ");//todo
          //  writer.Write('(');
          //  writer.Write(item.Value.invoke.ReturnType.ToDisplayString());
          //  writer.Write(')');
          //}

            writer.Write(item.Key).Write('(');
          if (item.Value.invoke.Parameters.Length > 0)
          {
            writer.Write('(');
            RenderStrings(item.Value.invoke.Parameters.Select(x => x.Type.ToDisplayString()).ToArray());
            writer.Write(")parameter");
          }

          if (!item.Value.invoke.ReturnsVoid)
          {
            writer.Write(")");
          }

          writer.Write(");");

          if (item.Value.invoke.ReturnsVoid)
          {
            writer.Write(" SendResponse(message);");
          }
        }

        writer.Write(" return true;");
        writer.WriteLine();
      }
    }

    private void RenderSymbol(TypeDefinition typeDefinition, ISymbol symbol, bool served)
    {
      switch (symbol)
      {
        case IMethodSymbol methodSymbol: RenderMethod(typeDefinition, methodSymbol, served); break;
        case IEventSymbol eventSymbol: RenderEvent(typeDefinition, eventSymbol, served); break;
        case IPropertySymbol propertySymbol: RenderProperty(typeDefinition, propertySymbol, served); break;
      }
    }

    private void RenderProperty(TypeDefinition typeDefinition, IPropertySymbol x, bool served) => writer
      .WriteLine($$"""//There was {{x.Kind}} {{x.Type.ToDisplayString()}} {{x.Name}}""").WriteLine();

    private void RenderEvent(TypeDefinition typeDefinition, IEventSymbol x, bool served) => writer
      .WriteLine($$"""
      {{() => RenderAttributes(x, served)}}
      {{() => RenderSignature(typeDefinition,  x, served)}}
      {{() => RenderBody(typeDefinition, x, served)}}
      """);

    private void RenderMethod(TypeDefinition typeDefinition, IMethodSymbol x, bool served) => writer
      .WriteLine($$"""
      {{() => RenderAttributes(x, served)}}
      {{() => RenderSignature(typeDefinition, x, served)}}
      {{() => RenderBody(typeDefinition, x, served)}}
      """);

    private void RenderAttributes(IEventSymbol x, bool served = false) => writer
      .Write("[System.ComponentModel.Description(\"")
      .Write(MethodKind.EventRaise)
      .Write("\")]");

    private void RenderAttributes(IMethodSymbol x, bool served = false)
    {
      writer.Write("[System.ComponentModel.Description(\"").Write(x.MethodKind).Write("\")]");
      switch (x.MethodKind)
      {
        case MethodKind.EventAdd:
        case MethodKind.EventRemove:
        case MethodKind.PropertyGet:
        case MethodKind.PropertySet:
        case MethodKind.Ordinary:
          {
            break;
          }
        default:
          {
            writer.Write("""[System.Obsolete("Not Generated")]""");
            break;
          }
      }
    }
    
    private void RenderBody(TypeDefinition typeDefinition, IEventSymbol eventSymbol, bool served)
    {
      var x = (IMethodSymbol)eventSymbol.Type.GetMembers().Where(x => x.Name == "Invoke").First();

      writer.WriteLine('{');
      writer.IncreaseIndent();
      if (served)
      {
        if (x.ReturnsVoid)
        {
          writer.Write($$"""{{() => RenderName(eventSymbol, true, "event_")}}?.Invoke({{()=>RenderStrings(x.Parameters.Select(x=>x.Name).ToArray())}});""");
        }
        else
        {
          writer.Write($$"""return {{() => RenderName(eventSymbol, true, "event_")}}?.Invoke({{() => RenderStrings(x.Parameters.Select(x => x.Name).ToArray())}}) ?? default;""");
        }
      }
      else
      {
        var (input, output) = GetIO(x);

        if (!x.ReturnsVoid || output.Count > 0)
          writer.Write("var result = ");

        writer.Write("Remote");

        if (!x.ReturnsVoid || output.Count > 0 || input.Count > 0)
        {
          writer.Write('<');

          if (input.Count > 1)
            writer.Write("(");
          RenderTypeParameters(input.Select(x => x.Type).ToArray());
          if (input.Count > 1)
            writer.Write(")");

          if ((!x.ReturnsVoid || output.Count > 0) && input.Count > 0)
            writer.Write(", ");

          if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
            writer.Write('(');
          if (!x.ReturnsVoid)
            writer.Write(x.ReturnType.ToDisplayString());
          RenderTypeParameters(output.Select(x => x.Type).ToArray(), !x.ReturnsVoid);
          if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
            writer.Write(')');

          writer.Write('>');
        }

        writer.Write("(\"");
        RenderName(eventSymbol, served, "invoke_");
        writer.Write('"');
        if (input.Count > 0)
          writer.Write(", ");
        if (input.Count > 1)
          writer.Write('(');
        RenderStrings(input.Select(x => x.Name).ToArray());
        if (input.Count > 1)
          writer.Write(')');
        writer.Write(");");

        if (!x.ReturnsVoid || output.Count > 0)
          writer.WriteLine();

        if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
        {
          for (int i = output.Count - 1; i >= 0; i--)
          {
            writer.Write($$"""{{output[i].Name}} = result.Item{{i + (x.ReturnsVoid ? 1 : 2)}};""");
          }
        }

        if (x.ReturnsVoid && output.Count == 1)
        {
          writer.Write($$"""{{output[0].Name}} = result;""");
        }

        if (!x.ReturnsVoid)
        {
          if (output.Count > 0)
            writer.Write("return result.Item1;");
          else
            writer.Write("return result;");
        }
      }
      writer.DecreaseIndent();
      writer.WriteLine();
      writer.WriteLine('}');
    }

    private void RenderBody(TypeDefinition typeDefinition, IMethodSymbol x, bool served)
    {
      writer.WriteLine('{');
      writer.IncreaseIndent();
      switch (x.MethodKind)
      {
        case MethodKind.EventAdd:
          {
            writer.Write($$"""(({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name.Substring(4)}} += value;""");
            break;
          }
        case MethodKind.EventRemove:
          {
            writer.Write($$"""(({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name.Substring(7)}} -= value;""");
            break;
          }
        case MethodKind.PropertyGet:
          {
            if (served)
            {
              writer.Write($$"""return Remote<{{x.ReturnType.ToDisplayString()}}>("{{()=> RenderName(x,served)}}");""");
            }
            else
            {
              writer.Write($$"""return (({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name.Substring(4)}};""");
            }
            break;
          }
        case MethodKind.PropertySet:
          {
            if (served)
            {
              writer.Write($$"""Remote<{{x.Parameters[0].Type.ToDisplayString()}}>("{{() => RenderName(x, served)}}", value);""");
            }
            else
            {
              writer.Write($$"""(({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name.Substring(4)}} = value;""");
            }
            break;
          }
        case MethodKind.Ordinary:
          {
            var (input, output) = GetIO(x);
            if (served)
            {
              if (!x.ReturnsVoid || output.Count > 0)
                writer.Write("var result = ");

              writer.Write("Remote");

              if (!x.ReturnsVoid || output.Count > 0 || input.Count > 0)
              {
                writer.Write('<');

                if (input.Count > 1)
                  writer.Write("(");
                RenderTypeParameters(input.Select(x => x.Type).ToArray());
                if (input.Count > 1)
                  writer.Write(")");

                if ((!x.ReturnsVoid || output.Count > 0) && input.Count > 0)
                  writer.Write(", ");

                if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
                  writer.Write('(');
                if (!x.ReturnsVoid)
                  writer.Write(x.ReturnType.ToDisplayString());
                RenderTypeParameters(output.Select(x => x.Type).ToArray(), !x.ReturnsVoid);
                if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
                  writer.Write(')');

                writer.Write('>');
              }

              writer.Write("(\"");
              RenderName(x, served);
              writer.Write('"');
              if(input.Count > 0)
                writer.Write(", ");
              if(input.Count > 1)
                writer.Write('(');
              RenderStrings(input.Select(x => x.Name).ToArray());
              if (input.Count > 1)
                writer.Write(')');
              writer.Write(");");

              if (!x.ReturnsVoid || output.Count > 0)
                writer.WriteLine();

              if ((!x.ReturnsVoid && output.Count > 0) || output.Count > 1)
              {
                for (int i = output.Count - 1; i >= 0; i--)
                {
                  writer.Write($$"""{{output[i].Name}} = result.Item{{i + (x.ReturnsVoid ? 1 : 2)}};""");
                }
              }

              if (x.ReturnsVoid && output.Count == 1)
              {
                writer.Write($$"""{{output[0].Name}} = result;""");
              }

              if (!x.ReturnsVoid)
              {
                if (output.Count > 0)
                  writer.Write("return result.Item1;");
                else
                  writer.Write("return result;");
              }
            }
            else
            {
              if (!x.ReturnsVoid)
                writer.Write("var result = ");

              writer.Write($$"""(({{x.ContainingType.ToDisplayString()}}?) Target)!.{{x.Name}}""");

              if(x.TypeParameters.Length > 0)
              {
                writer.Write('<');
                RenderTypeParameters(x.TypeParameters);
                writer.Write('>');
              }

              writer.Write('(');
              for (int i = 0; i < x.Parameters.Length; i++)
              {
                var parameter = x.Parameters[i];
                switch (parameter.RefKind)
                {
                  case RefKind.None when input.Count > 1: writer.Write("parameter."); break;
                  case RefKind.Ref  when input.Count > 1: writer.Write("ref parameter."); break;
                  case RefKind.Ref: writer.Write("ref "); break;
                  case RefKind.Out: writer.Write("out var "); break;
                  case RefKind.In when input.Count > 1:
                  case RefKind.In + 1 when input.Count > 1: writer.Write("ref parameter."); break;
                  case RefKind.In:
                  case RefKind.In + 1: writer.Write("in "); break;
                }
                writer.Write(parameter.Name);
                if (i != x.Parameters.Length - 1)
                {
                  writer.Write(", ");
                }
              }
              writer.Write(");");

              if (!x.ReturnsVoid || output.Count > 0)
              {
                writer.WriteLine();

                if (!x.ReturnsVoid)
                {
                  if (output.Count == 0)
                    writer.Write("return result;");
                  else
                  {
                    writer.Write("return (result, ");
                    for (int i = 0; i < output.Count; i++)
                    {
                      var parameter = output[i];
                      switch (parameter.RefKind)
                      {
                        case RefKind.None when input.Count > 1:
                        case RefKind.Ref when input.Count > 1: writer.Write("parameter."); break;
                        case RefKind.Ref: break;
                        case RefKind.Out: break;
                        case RefKind.In when input.Count > 1:
                        case RefKind.In + 1 when input.Count > 1: writer.Write("parameter."); break;
                        case RefKind.In:
                        case RefKind.In + 1: break;
                      }
                      writer.Write(parameter.Name);
                      if (i != output.Count - 1)
                      {
                        writer.Write(", ");
                      }
                    }
                    writer.Write(");");
                  }
                }
                else
                {
                  if (output.Count == 1)
                  {
                    writer.Write("return ");
                    writer.Write(output[0].Name);
                    writer.Write(';');
                  }
                  else
                  {
                    writer.Write("return (");
                    for (int i = 0; i < output.Count; i++)
                    {
                      var parameter = output[i];
                      switch (parameter.RefKind)
                      {
                        case RefKind.None when input.Count > 1:
                        case RefKind.Ref when input.Count > 1: writer.Write("parameter."); break;
                        case RefKind.Ref: break;
                        case RefKind.Out: break;
                        case RefKind.In when input.Count > 1:
                        case RefKind.In + 1 when input.Count > 1: writer.Write("parameter."); break;
                        case RefKind.In:
                        case RefKind.In + 1: break;
                      }
                      writer.Write(parameter.Name);
                      if (i != output.Count - 1)
                      {
                        writer.Write(", ");
                      }
                    }
                    writer.Write(");");
                  }
                }
              }
            }
            break;
          }
        default:
          {
            writer.Write("throw new NotImplementedException();");
            break;
          }
      }
      writer.DecreaseIndent();
      writer.WriteLine();
      writer.WriteLine('}');
    }

    private void RenderTypeName(INamedTypeSymbol x, bool served)
    {
      writer
        //.Write(served?"Serve_":"Using_")
        .Write(x.Name);
      if (x.Arity > 0)
        writer.Write(x.Arity);
    }

    private void RenderName(ISymbol x, bool served, string prefix = "")
    {
      RenderTypeName(x.ContainingType, served);
      writer
        .Write('_')
        .Write(prefix)
        .Write(x.Name);
    }

    private string GetName(ISymbol x, bool served, string prefix = "")
    {
      var sb = new StringBuilder();
      sb.Append(x.ContainingType.Name);
      if (x.ContainingType.Arity > 0)
        sb.Append(x.ContainingType.Arity);
      sb.Append('_');
      sb.Append(prefix);
      sb.Append(x.Name);
      return sb.ToString();
    }

    private void RenderSignature(TypeDefinition typeDefinition, IEventSymbol x, bool served)
    {
      writer.Write("internal ");
      if (x.IsStatic)
        writer.Write("static ");

      var invoke = (IMethodSymbol)x.Type.GetMembers().Where(x => x.Name == "Invoke").First();
      //var beginInvoke = (IMethodSymbol)x.Type.GetMembers().Where(x => x.Name == "BeginInvoke").First();

      writer
        .Write(invoke.ReturnType.ToDisplayString())//todo actual return type of event
        .Write(' ');
      RenderName(x, served, "invoke_");
      writer.Write('(');
      RenderParameters(invoke.Parameters);
      writer.Write(')');

      if(served)
        typeDefinition.Commands[GetName(x, served, "invoke_")] = (x,invoke);
    }

    private void RenderSignature(TypeDefinition typeDefinition, IMethodSymbol x, bool served)
    {
      writer.Write("internal ");
      if (x.IsStatic)
        writer.Write("static ");
      if (x.IsAsync)
        writer.Write("async ");

      if (served)
      {
        writer
          .Write(x.ReturnType.ToDisplayString())
          .Write(' ');
        RenderName(x, served);
        if (x.TypeParameters.Length > 0)
        {
          writer.Write('<');
          RenderTypeParameters(x.TypeParameters);
          writer.Write('>');
        }
        writer.Write('(');
        RenderParameters(x.Parameters);
        writer.Write(')');
      }
      else
      {
        var (input, output) = GetIO(x);
        if(x.ReturnsVoid && output.Count == 0)
        {
          writer.Write("void");
        }
        else if(x.ReturnsVoid && output.Count == 1)
        {
          writer.Write(output[0].Type.ToDisplayString());
        }
        else if (!x.ReturnsVoid && output.Count == 0)
        {
          writer.Write(x.ReturnType.ToDisplayString());
        }
        else
        {
          writer.Write('(');
          if(!x.ReturnsVoid)
            writer.Write(x.ReturnType.ToDisplayString());
          if(!x.ReturnsVoid && output.Count > 0)
            writer.Write(", ");
          RenderStrings(output.Select(x=>x.Type.ToDisplayString()).ToArray());
          writer.Write(')');
        }
        writer.Write(' ');
        RenderName(x, served);
        if (x.TypeParameters.Length > 0)
        {
          writer.Write('<');
          RenderTypeParameters(x.TypeParameters);
          writer.Write('>');
        }
        writer.Write('(');
        if(input.Count>1)
          writer.Write('(');
        for (int i = 0; i < input.Count; i++)
        {
          var parameter = input[i];
          writer.Write(parameter.Type.ToDisplayString());
          writer.Write(' ');
          writer.Write(parameter.Name);
          if (i != input.Count - 1)
          {
            writer.Write(", ");
          }
        }
        if (input.Count > 1)
          writer.Write(") parameter");
        writer.Write(')');

        if (x.TypeParameters.Length == 0)
          typeDefinition.Commands[GetName(x, served)] = (x, x);
      }
    }

    private (List<IParameterSymbol> input, List<IParameterSymbol> output) GetIO(IMethodSymbol x)
    {
      var input = new List<IParameterSymbol>();
      var output = new List<IParameterSymbol>();

      foreach (var parameter in x.Parameters)
      {
        switch (parameter.RefKind)
        {
          case RefKind.None: input.Add(parameter); break;
          case RefKind.Ref: input.Add(parameter); output.Add(parameter); break;
          case RefKind.Out: output.Add(parameter); break;
          case RefKind.In:
          case RefKind.In + 1: input.Add(parameter); break;
        }
      }
      return (input, output);
    }

    private void RenderTypeParameters(IReadOnlyList<ISymbol> symbols, bool leadingComma = false)
    {
      if (symbols.Count > 0 && leadingComma)
        writer.Write(", ");

      for (int i = 0; i < symbols.Count; i++)
      {
        var symbol = symbols[i];
        writer.Write(symbol.ToDisplayString());
        if (i != symbols.Count - 1)
        {
          writer.Write(", ");
        }
      }
    }

    private void RenderStrings(IReadOnlyList<string> parameters, bool leadingComma = false)
    {
      if (parameters.Count > 0 && leadingComma)
        writer.Write(", ");

      for (int i = 0; i < parameters.Count; i++)
      {
        var parameter = parameters[i];
        writer.Write(parameter);
        if (i != parameters.Count - 1)
        {
          writer.Write(", ");
        }
      }
    }

    private void RenderParameters(IReadOnlyList<ISymbol> parameters, bool leadingComma = false)
    {
      if(parameters.Count > 0 && leadingComma)
        writer.Write(", ");

      for (int i = 0; i < parameters.Count; i++)
      {
        var parameter = parameters[i];
        writer.Write(parameter.ToDisplayString());
        if(i != parameters.Count - 1)
        {
          writer.Write(", ");
        }
      }
    }
  }
}
