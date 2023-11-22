namespace T.Pipes.Test.Abstractions
{
  public interface IAbstract<T>
  {
    event Func<T, T> Map;

    /// <summary>
    /// T GetT();
    /// </summary>
    /// <returns></returns>
    T GetT();

    /// <summary>
    /// void SetT(T);
    /// </summary>
    /// <param name="value"></param>
    void SetT(T value);

    /// <summary>
    /// T get_Tea();
    /// void set_Tea(T);
    /// </summary>
    T Tea { get; set; }

    /// <summary>
    /// (T,T) GetTs();
    /// </summary>
    /// <returns></returns>
    (T a, T b) GetTs();

    /// <summary>
    /// T[] GetTearr();
    /// </summary>
    /// <returns></returns>
    T[] GetTearr();

    /// <summary>
    /// void SetTs(T[]);
    /// </summary>
    /// <param name="a"></param>
    void SetTs(T[] a);

    /// <summary>
    /// T RefT(T);
    /// </summary>
    /// <param name="value"></param>
    void RefT(ref T value);

    /// <summary>
    /// T OutT();
    /// </summary>
    /// <param name="value"></param>
    void OutT(out T value);

    /// <summary>
    /// void InT(in T);
    /// </summary>
    /// <param name="value"></param>
    void InT(in T value);
  }
}
