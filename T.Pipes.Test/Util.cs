using FluentAssertions.Specialized;
using System.Threading.Tasks;

namespace T.Pipes.Test
{
  internal delegate TTask CachedTask<TTask>() where TTask : Task; 

  internal static class LazyUtil
  {
    public static CachedTask<V> Cache<K,V>(K source, Func<K,V> func) where V : Task 
      => source.Cache(func);

    public static CachedTask<T> Cache<T>(Func<T> func) where T : Task 
      => func.Cache();
  }

  internal static class Util
  {
    public static CachedTask<V> Cache<K, V>(this K source, Func<K, V> func) where V : Task => Cache(() => func(source));

    public static CachedTask<T> Cache<T>(this Func<T> func) where T : Task
    {
      var lazy = new Lazy<T>(func, LazyThreadSafetyMode.PublicationOnly);
      return () => lazy.Value;
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

    public static Task<AndConstraint<TAssertions>> BeCompletedSuccessfullyAsync<TTask, TAssertions>
      (this TAssertions me, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
      => BeCompletedSuccessfullyWithinAsync<TTask, TAssertions>(me, Timeout.InfiniteTimeSpan, because, becauseArgs);

    public static async Task<AndConstraint<TAssertions>> BeCompletedSuccessfullyWithinAsync<TTask, TAssertions>
      (this TAssertions me, TimeSpan timeout, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
    {
      var task = me.Subject.Cache();
      await task.Awaiting<TTask,Task>().Should().CompleteWithinAsync(timeout, because, becauseArgs);
#if NET5_0_OR_GREATER
      task().IsCompletedSuccessfully.Should().BeTrue(because, becauseArgs);
#else
      task().IsCompleted.Should().BeTrue(because, becauseArgs);
      task().IsCanceled.Should().BeFalse(because, becauseArgs);
      task().IsFaulted.Should().BeFalse(because, becauseArgs);
#endif
      return new AndConstraint<TAssertions>(me);
    }

    public static Task<AndConstraint<TAssertions>> BeCanceledAsync<TTask, TAssertions>
      (this TAssertions me, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
      => BeCanceledWithinAsync<TTask, TAssertions>(me, Timeout.InfiniteTimeSpan, because, becauseArgs);

    public static async Task<AndConstraint<TAssertions>> BeCanceledWithinAsync<TTask, TAssertions>
      (this TAssertions me, TimeSpan timeout, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
    {
      var task = me.Subject.AsLazy<Task>();
      await task.Should().CompleteWithinAsync(timeout, because, becauseArgs);
      task().IsCanceled.Should().BeTrue(because, becauseArgs);
      return new AndConstraint<TAssertions>(me);
    }

    public static Task<AndConstraint<TAssertions>> BeFaultedAsync<TTask, TAssertions>
      (this TAssertions me, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
      => BeFaultedWithinAsync<TTask, TAssertions>(me, Timeout.InfiniteTimeSpan, because, becauseArgs);

    public static async Task<AndConstraint<TAssertions>> BeFaultedWithinAsync<TTask, TAssertions>
      (this TAssertions me, TimeSpan timeout, string because = "", params object[] becauseArgs)
      where TTask : Task
      where TAssertions : AsyncFunctionAssertions<TTask, TAssertions>
    {
      var task = me.Subject.AsLazy<Task>();
      await task.Should().CompleteWithinAsync(timeout, because, becauseArgs);
      task().IsFaulted.Should().BeTrue(because, becauseArgs);
      return new AndConstraint<TAssertions>(me);
    }
  }
}
