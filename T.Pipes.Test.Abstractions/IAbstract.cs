namespace T.Pipes.Test.Abstractions
{
  public interface IAbstract
  {
    event Action? Act;
    event Func<int>? Set;
    event Action<string>? Get;

    /// <summary>
    /// get/set
    /// add/remove
    /// </summary>
    Func<int, int>? Tea { get; set; }

    /// <summary>
    /// void Action();
    /// </summary>
    void Action();

    /// <summary>
    /// int GetInt();
    /// </summary>
    /// <returns></returns>
    int GetInt();

    /// <summary>
    /// void SetInt(int);
    /// </summary>
    /// <param name="value"></param>
    void SetInt(int value);

    /// <summary>
    /// int get_Int();
    /// void set_Int(int);
    /// </summary>
    int Int { get; set; } 

    /// <summary>
    /// (string,string) GetStrings();
    /// </summary>
    /// <returns></returns>
    (string a, string b) GetStrings();

    /// <summary>
    /// int[] GetInts();
    /// </summary>
    /// <returns></returns>
    int[] GetInts();

    /// <summary>
    /// void SetInts(int[]);
    /// </summary>
    /// <param name="a"></param>
    void SetInts(int[] a);

    /// <summary>
    /// int RefInt(int);
    /// </summary>
    /// <param name="value"></param>
    void RefInt(ref int value);

    /// <summary>
    /// int OutInt();
    /// </summary>
    /// <param name="value"></param>
    void OutInt(out int value);

    /// <summary>
    /// void InInt(in int value);
    /// </summary>
    /// <param name="value"></param>
    void InInt(in int value);

    int DoIt(int a, int b, int c, out int d, out int e);

    void RefIt(ref int a, ref int b, ref int c);

    TValue Map<TValue>(TValue from);
    TOut ReMap<TIn,TOut>(TIn from);
    TOut UnMap<TOut,TIn>(TIn from);

    int? MaybeInt(int? a);

    string? MaybeString(string? a);
  }
}
