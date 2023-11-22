using H.Pipes;
using T.Pipes.Abstractions;
using T.Pipes.Test.Abstractions;

namespace T.Pipes.Test.Server
{
  [PipeServe(typeof(IAbstract))]
  [PipeServe(typeof(IAbstract<int>))]
  internal partial class Callback
    : DelegatingPipeMessageCallback<IPipeServer<PipeMessage>, DelegatingServerAuto>
  {
    public Callback(IPipeServer<PipeMessage> pipe) : base(pipe)
    {
    }

    internal partial int IAbstract1_invoke_Map(int arg) => Target!._map?.Invoke(arg) ?? default;
    internal partial void IAbstract_invoke_Act() => Target!._act?.Invoke();
    internal partial int IAbstract_invoke_Set() => Target!._set?.Invoke() ?? default;
    internal partial void IAbstract_invoke_Get(string obj) => Target!._get?.Invoke(obj);
  }

  internal class DelegatingServerAuto 
    : DelegatingPipeServer<PipeMessage, PipeMessageFactory, DelegatingServerAuto, Callback>, IAbstract, IAbstract<int>
  {
    public DelegatingServerAuto(string pipeName) : this(new PipeServer<PipeMessage>(pipeName))
    {
    }

    public DelegatingServerAuto(IPipeServer<PipeMessage> pipe) : base(pipe, new(pipe))
    {
      Callback.Target = this;
    }

    public Func<int, int>? Tea { get => Callback.IAbstract_get_Tea(); set => Callback.IAbstract_set_Tea(value); }
    public int Int { get => Callback.IAbstract_get_Int(); set => Callback.IAbstract_set_Int(value); }
    int IAbstract<int>.Tea { get => Callback.IAbstract1_get_Tea(); set => Callback.IAbstract1_set_Tea(value); }

    internal Action? _act;
    public event Action? Act
    {
      add => _act += value;
      remove => _act -= value;
    }
    internal Func<int>? _set;
    public event Func<int>? Set
    {
      add => _set += value;
      remove => _set -= value;
    }
    internal Action<string>? _get;
    public event Action<string>? Get
    {
      add => _get += value;
      remove => _get -= value;
    }
    internal Func<int, int>? _map;
    public event Func<int, int> Map
    {
      add => _map += value;
      remove => _map -= value;
    }

    public void Action() => Callback.IAbstract_Action();

    public int DoIt(int a, int b, int c, out int d, out int e) => Callback.IAbstract_DoIt(a, b, c, out d, out e);
    public int GetInt() => Callback.IAbstract_GetInt();
    public int[] GetInts() => Callback.IAbstract_GetInts();
    public (string a, string b) GetStrings() => Callback.IAbstract_GetStrings();

    public int GetT() => Callback.IAbstract1_GetT();

    public int[] GetTearr() => Callback.IAbstract1_GetTearr();

    public (int a, int b) GetTs() => Callback.IAbstract1_GetTs();

    public void InInt(in int value) => Callback.IAbstract_InInt(value);

    public void InT(in int value) => Callback.IAbstract1_InT(value);

    public int? MaybeInt(int? a) => Callback.IAbstract_MaybeInt(a);

    public string? MaybeString(string? a) => Callback.IAbstract_MaybeString(a);

    public void OutInt(out int value) => Callback.IAbstract_OutInt(out value);

    public void OutT(out int value) => Callback.IAbstract1_OutT(out value);

    public void RefInt(ref int value) => Callback.IAbstract_RefInt(ref value);
    public void RefIt(ref int a, ref int b, ref int c) => Callback.IAbstract_RefIt(ref a, ref b, ref c);

    public void RefT(ref int value) => Callback.IAbstract1_RefT(ref value);

    public void SetInt(int value) => Callback.IAbstract_SetInt(value);
    public void SetInts(int[] a) => Callback.IAbstract_SetInts(a);

    public void SetT(int value) => Callback.IAbstract1_SetT(value);

    public void SetTs(int[] a) => Callback.IAbstract1_SetTs(a);
  }
}
