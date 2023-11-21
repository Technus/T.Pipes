﻿using System;
using System.ComponentModel;

namespace T.Pipes.Abstractions
{
  [Description("Used by T.Pipes.SourceGeneration")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = true)]
  public class PipeServeAttribute : Attribute
  {
    public PipeServeAttribute() { }

    public PipeServeAttribute(string name) => Name = name;
    public PipeServeAttribute(Type type) => Type = type;

    /// <summary>
    /// Fully qualified names to write helpers for
    /// </summary>
    public string Name { get; } = string.Empty;


    /// <summary>
    /// The Type to write helpers for
    /// </summary>
    public Type Type { get; } = typeof(void);
  }
}
