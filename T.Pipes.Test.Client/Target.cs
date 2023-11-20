using T.Pipes.Test.Abstractions;

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

    public int GetInt() => i;

    public int[] GetInts() => j;

    public (string a, string b) GetStrings() => ("John", "Paul");

    public void InInt(in int value) => i = value;

    public void OutInt(out int value) => value = i;

    public void RefInt(ref int value) => value *= 2;

    public void SetInt(int value) => i = value;

    public void SetInts(int[] a) => j = a;
  }
}
