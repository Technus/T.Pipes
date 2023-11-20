using H.Pipes;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class DelegatingServer : DelegatingPipeServer<PipeMessage, PipeMessageFactory>, IAbstract
  {
    public DelegatingServer(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingServer(IPipeServer<PipeMessage> pipe) : base(new PipeMessageFactory(), pipe)
    {
      AddFunctionRemote(x => { Act?.Invoke(); return null; }, nameof(IAbstract.Act));
      AddFunctionRemote(x => Set?.Invoke(), nameof(IAbstract.Set));
      AddFunctionRemote(x => { Get?.Invoke(x as string ?? string.Empty); return null; }, nameof(IAbstract.Get));
    }

    public Func<int, int>? Tea { get => GetRemote<Func<int, int>?>(); set => SetRemote<Func<int, int>?>(value); }
    public int Int { get => GetRemote<int>(); set => SetRemote(value); }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public void Action() => InvokeRemote();
    public int DoIt(int a, int b, int c, out int d, out int e)
    {
      var (ret, dOut, eOut) = InvokeRemote<(int, int, int)>((a, b, c));
      d = dOut; e = eOut; return ret;
    }
    public int GetInt() => InvokeRemote<int>();
    public int[] GetInts() => InvokeRemote<int[]>() ?? Array.Empty<int>();
    public (string a, string b) GetStrings() => InvokeRemote<(string a, string b)>();
    public void InInt(in int value) => InvokeRemote(value);
    public void OutInt(out int value) => value = InvokeRemote<int>();
    public void RefInt(ref int value) => value = InvokeRemote<int>(value);
    public void SetInt(int value) => InvokeRemote(value);
    public void SetInts(int[] a) => InvokeRemote(a);
  }
}
