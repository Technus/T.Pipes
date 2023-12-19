using FluentAssertions.Specialized;

namespace T.Pipes.Test
{
  internal delegate TTask CachedTask<TTask>() where TTask : Task;

  [ExcludeFromCodeCoverage]
  internal static class LazyUtil
  {
    public static CachedTask<V> Cache<K,V>(K source, Func<K,V> func) where V : Task
      => source.Cache(func);

    public static CachedTask<T> Cache<T>(Func<T> func) where T : Task
      => func.Cache();
  }

  [ExcludeFromCodeCoverage]
  internal static class Util
  {
    public static CachedTask<V> Cache<K, V>(this K source, Func<K, V> func) where V : Task => Cache(() => func(source));

    public static CachedTask<T> Cache<T>(this Func<T> func) where T : Task
    {
      var lazy = new Lazy<T>(func, LazyThreadSafetyMode.PublicationOnly);
      return () => lazy.Value;
    }

    public static NonGenericAsyncFunctionAssertions Should(this ValueTask value)
    {
      var action = () => value.AsTask();
      return action.Should();
    }

    public static GenericAsyncFunctionAssertions<T> Should<T>(this ValueTask<T> value)
    {
      var action = () => value.AsTask();
      return action.Should();
    }

    public static NonGenericAsyncFunctionAssertions Should(this Task value)
    {
      var action = () => value;
      return action.Should();
    }

    public static GenericAsyncFunctionAssertions<T> Should<T>(this Task<T> value)
    {
      var action = () => value;
      return action.Should();
    }

    public static NonGenericAsyncFunctionAssertions Should(this CachedTask<Task> value)
    {
      var action = () => value();
      return action.Should();
    }

    public static GenericAsyncFunctionAssertions<T> Should<T>(this CachedTask<Task<T>> value)
    {
      var action = () => value();
      return action.Should();
    }

    public static Func<TTask> Awaiting<TActualTask,TTask>(this CachedTask<TActualTask> value) where TActualTask : TTask where TTask : Task 
      => () => value();

    public static Func<TTask> Awaiting<TTask>(this CachedTask<TTask> value) where TTask : Task 
      => () => value();

    public static Func<TTask> Awaiting<TTask>(this TTask subject) where TTask : Task 
      => () => subject;
  }
}
