using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Client
{
  internal class Target : IAbstract
  {
    public Func<int, int>? Tea { get; set; }
    public int Int { get; set; }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public void Action()
    {
      throw new NotImplementedException();
    }

    public int GetInt()
    {
      throw new NotImplementedException();
    }

    public int[] GetInts()
    {
      throw new NotImplementedException();
    }

    public (string a, string b) GetStrings()
    {
      throw new NotImplementedException();
    }

    public void InInt(in int value)
    {
      throw new NotImplementedException();
    }

    public void OutInt(out int value)
    {
      throw new NotImplementedException();
    }

    public void RefInt(ref int value)
    {
      throw new NotImplementedException();
    }

    public void SetInt(int value)
    {
      throw new NotImplementedException();
    }

    public void SetInts(int[] a)
    {
      throw new NotImplementedException();
    }
  }
}
