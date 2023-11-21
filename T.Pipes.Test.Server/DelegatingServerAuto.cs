using H.Pipes;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<int>))]
  internal partial class Callback<TTarget> 
    : DelegatingPipeMessageCallback<IPipeServer<PipeMessage>, TTarget> 
    where TTarget : IAbstract, IAbstract<int>
  {
    public Callback(IPipeServer<PipeMessage> pipe) : base(pipe)
    {
    }
  }

  internal class DelegatingServerAuto 
    : DelegatingPipeServer<PipeMessage, PipeMessageFactory, DelegatingServerAuto, Callback<DelegatingServerAuto>>, IAbstract, IAbstract<int>
  {
    public DelegatingServerAuto(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingServerAuto(IPipeServer<PipeMessage> pipe) : base(pipe, new(pipe))
    {
      Callback.Target = this;
    }

    public Func<int, int>? Tea { get => GetRemote<Func<int, int>?>(); set => SetRemote<Func<int, int>?>(value); }
    public int Int { get => GetRemote<int>(); set => SetRemote(value); }
    int IAbstract<int>.Tea { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action? Act;
    public event Func<int>? Set;
    public event Action<string>? Get;

    event Func<int, int> IAbstract<int>.Map
    {
      add
      {
        throw new NotImplementedException();
      }

      remove
      {
        throw new NotImplementedException();
      }
    }

    public void Action() => InvokeRemote();

    public int DoIt(int a, int b, int c, out int d, out int e)
    {
      (var ret, d, e) = InvokeRemote<(int, int, int)>((a, b, c));
      return ret;
    }
    public int GetInt() => InvokeRemote<int>();
    public int[] GetInts() => InvokeRemote<int[]>() ?? Array.Empty<int>();
    public (string a, string b) GetStrings() => InvokeRemote<(string a, string b)>();

    public int GetT() => throw new NotImplementedException();

    public int[] GetTearr() => throw new NotImplementedException();

    public (int a, int b) GetTs() => throw new NotImplementedException();

    public void InInt(in int value) => InvokeRemote(value);

    public void InT(in int value)
    {
      throw new NotImplementedException();
    }

    public T Map<T>(T from) => throw new NotImplementedException();

    public int? MaybeInt(int? a)
    {
      throw new NotImplementedException();
    }

    public string? MaybeString(string? a)
    {
      throw new NotImplementedException();
    }

    public void OutInt(out int value) => value = InvokeRemote<int>();

    public void OutT(out int value)
    {
      throw new NotImplementedException();
    }

    public void RefInt(ref int value) => value = InvokeRemote<int>(value);
    public void RefIt(ref int a, ref int b, ref int c) => (a, b, c) = InvokeRemote<(int, int, int)>((a, b, c));

    public void RefT(ref int value)
    {
      throw new NotImplementedException();
    }

    public Tea ReMap<T, Tea>(T from) => throw new NotImplementedException();

    public Tea ReMap<Tea>(int from)
    {
      throw new NotImplementedException();
    }

    public void SetInt(int value) => InvokeRemote(value);
    public void SetInts(int[] a) => InvokeRemote(a);

    public void SetT(int value)
    {
      throw new NotImplementedException();
    }

    public void SetTs(int[] a)
    {
      throw new NotImplementedException();
    }

    public T UnMap<T, Tea>(Tea from) => throw new NotImplementedException();

    public int UnMap<Tea>(Tea from)
    {
      throw new NotImplementedException();
    }
  }
}
