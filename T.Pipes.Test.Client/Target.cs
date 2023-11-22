using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class Target : IAbstract, IAbstract<short>
  {


    private int i = 2137;
    private int[] j = [69, 420, 1337, 2137];
    private short[] s = [69, 420, 1337, 2137];

    public Target()
    {
      Act?.Invoke();
      Set?.Invoke();
      Get?.Invoke("");
      Map?.Invoke(2137);
    }

    public Func<int, int>? Tea { get; set; }
    public int Int { get; set; } = 1337;
    short IAbstract<short>.Tea { get ; set; }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public event Func<short, short>? Map;

    public void Action() { }

    public void Dispose()
    {
    }

    public int DoIt(int a, int b, int c, out int d, out int e)
    {
      d = a;
      e = b;
      return c;
    }

    public int GetInt() => i;

    public int[] GetInts() => j;

    public (string a, string b) GetStrings() => ("John", "Paul");
    public short GetT() => 2137;
    public short[] GetTearr() => [2,1,3,7];
    public (short a, short b) GetTs() => (21,37);
    public void InInt(in int value) => i = value;
    public void InT(in short value) => i = value;

    public int? MaybeInt(int? a) => a;

    public string? MaybeString(string? a) => a;

    public void OutInt(out int value) => value = i;
    public void OutT(out short value) => value = (short)i;
    public void RefInt(ref int value) => value *= 2;

    public void RefIt(ref int a, ref int b, ref int c) { a = 21; b = 37; c = 2137; }

    public void RefT(ref short value) => value *= 2;
    public void SetInt(int value) => i = value;

    public void SetInts(int[] a) => j = a;
    public void SetT(short value) => i = value;
    public void SetTs(short[] a) => s = a;
  }
}
