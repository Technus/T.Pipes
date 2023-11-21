using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class TargetProxy : IAbstract
  {
    public Func<int, int>? Tea { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int Int { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public void Action() => throw new NotImplementedException();

    public int DoIt(int a, int b, int c, out int d, out int e) => throw new NotImplementedException();

    public int GetInt() => throw new NotImplementedException();

    public int[] GetInts() => throw new NotImplementedException();

    public (string a, string b) GetStrings() => throw new NotImplementedException();

    public void InInt(in int value) => throw new NotImplementedException();
    public T Map<T>(T from) => throw new NotImplementedException();

    public int? MaybeInt(int? a) => throw new NotImplementedException();

    public string? MaybeString(string? a) => throw new NotImplementedException();

    public void OutInt(out int value) => throw new NotImplementedException();

    public void RefInt(ref int value) => throw new NotImplementedException();

    public void RefIt(ref int a, ref int b, ref int c) => throw new NotImplementedException();
    public Tea ReMap<T, Tea>(T from) => throw new NotImplementedException();
    public void SetInt(int value) => throw new NotImplementedException();

    public void SetInts(int[] a) => throw new NotImplementedException();
    public T UnMap<T, Tea>(Tea from) => throw new NotImplementedException();
  }
}
