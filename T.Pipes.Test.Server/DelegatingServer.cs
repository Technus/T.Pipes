using H.Pipes;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  internal class DelegatingServer : DelegatingPipeServer<PipeMessage, PipeMessageFactory, DelegatingServer>, IAbstract
  {
    public DelegatingServer(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingServer(IPipeServer<PipeMessage> pipe) : base(pipe, new PipeMessageFactory())
    {
      SetFunctionRemote(x => { Act?.Invoke(); return default; }, nameof(IAbstract.Act));
      SetFunctionRemote(x => Set?.Invoke(), nameof(IAbstract.Set));
      SetFunctionRemote(x => { Get?.Invoke(x as string ?? string.Empty); return default; }, nameof(IAbstract.Get));
    }

    public Func<int, int>? Tea { get => GetRemote<Func<int, int>?>(); set => SetRemote(value); }
    public int Int { get => GetRemote<int>(); set => SetRemote(value); }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    public void Action() => InvokeRemote();
    public int DoIt(int a, int b, int c, out int d, out int e)
    {
      (var ret, d, e) = InvokeRemote<(int, int, int)>((a, b, c));
      return ret;
    }
    public int GetInt() => InvokeRemote<int>();
    public int[] GetInts() => InvokeRemote<int[]>() ?? Array.Empty<int>();
    public (string a, string b) GetStrings() => InvokeRemote<(string a, string b)>();
    public void InInt(in int value) => InvokeRemote(value);
    public T Map<T>(T from) => throw new NotImplementedException();

    public int? MaybeInt(int? a) => throw new NotImplementedException();

    public string? MaybeString(string? a) => throw new NotImplementedException();

    public void OutInt(out int value) => value = InvokeRemote<int>();
    public void RefInt(ref int value) => value = InvokeRemote<int>(value);
    public void RefIt(ref int a, ref int b, ref int c) => (a, b, c) = InvokeRemote<(int, int, int)>((a, b, c));
    public Tea ReMap<T, Tea>(T from) => throw new NotImplementedException();
    public void SetInt(int value) => InvokeRemote(value);
    public void SetInts(int[] a) => InvokeRemote(a);
    public T UnMap<T, Tea>(Tea from) => throw new NotImplementedException();
  }
}
