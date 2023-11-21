using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = true)]
  public class PipeUseAttribute : Attribute
  {
    public PipeUseAttribute() { }

    public PipeUseAttribute(string name) => Name = name;
    public PipeUseAttribute(Type type) => Type = type;

    public string Name { get; } = string.Empty;
    public Type Type { get; } = typeof(void);
  }
}
