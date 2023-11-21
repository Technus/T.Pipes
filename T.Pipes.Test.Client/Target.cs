﻿using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class Target : IAbstract
  {
    private int i = 2137;
    private int[] j = [69, 420, 1337, 2137];

    public Func<int, int>? Tea { get; set; }
    public int Int { get; set; } = 1337;

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public void Action() { }

    public int DoIt(int a, int b, int c, out int d, out int e)
    {
      d = a;
      e = b;
      return c;
    }

    public int GetInt() => i;

    public int[] GetInts() => j;

    public (string a, string b) GetStrings() => ("John", "Paul");

    public void InInt(in int value) => i = value;
    public T Map<T>(T from) => from;

    public int? MaybeInt(int? a) => a;

    public string? MaybeString(string? a) => a;

    public void OutInt(out int value) => value = i;

    public void RefInt(ref int value) => value *= 2;

    public void RefIt(ref int a, ref int b, ref int c) { a = 21; b = 37; c = 2137; }

    public Tea ReMap<T, Tea>(T from) => throw new NotImplementedException();
    public void SetInt(int value) => i = value;

    public void SetInts(int[] a) => j = a;
    public T UnMap<T, Tea>(Tea from) => throw new NotImplementedException();
  }
}
