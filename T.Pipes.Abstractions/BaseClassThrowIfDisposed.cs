using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
  public abstract partial class BaseClass
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
  {
    /// <summary>
    /// Throws error when it is disposed or being disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed()
    {
      if (IsDisposed)
      {
        throw new ObjectDisposedException(GetType().Name, $"Disposed Check, Dispose State is: {(DisposeState)_disposeState}");
      }
    }

    /// <summary>
    /// Throws error when it is disposed or being disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed(string message)
    {
      if (IsDisposed)
      {
        throw new ObjectDisposedException(GetType().Name, message);
      }
    }

    /// <summary>
    /// Throws error when it is disposed or being disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed(IFormattable message)
    {
      if (IsDisposed)
      {
        throw new ObjectDisposedException(GetType().Name, message.ToString());
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed(Func<bool> predicate)
    {
      ThrowIfDisposed();
      if (predicate())
      {
        throw new ObjectDisposedException(GetType().Name, $"Predicate Check, Dispose State is: {(DisposeState)_disposeState}");
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="message"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed(Func<bool> predicate, string message)
    {
      ThrowIfDisposed(message);
      if (predicate())
      {
        throw new ObjectDisposedException(GetType().Name, message);
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="message"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed(Func<bool> predicate, IFormattable message)
    {
      ThrowIfDisposed(message);
      if (predicate())
      {
        throw new ObjectDisposedException(GetType().Name, message.ToString());
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate">using this as a parameter so maybe it can be a static lambda</param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed<T>(Func<T, bool> predicate) where T : BaseClass
    {
      ThrowIfDisposed();
      if (predicate((T)this))
      {
        throw new ObjectDisposedException(GetType().Name, $"Function Predicate Check, Dispose State is: {(DisposeState)_disposeState}");
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate">using this as a parameter so maybe it can be a static lambda</param>
    /// <param name="message"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed<T>(Func<T, bool> predicate, string message) where T : BaseClass
    {
      ThrowIfDisposed(message);
      if (predicate((T)this))
      {
        throw new ObjectDisposedException(GetType().Name, message);
      }
    }

    /// <summary>
    /// This also throws when the supplied predicate is true
    /// </summary>
    /// <param name="predicate">using this as a parameter so maybe it can be a static lambda</param>
    /// <param name="message"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    protected virtual void ThrowIfDisposed<T>(Func<T, bool> predicate, IFormattable message) where T : BaseClass
    {
      ThrowIfDisposed(message);
      if (predicate((T)this))
      {
        throw new ObjectDisposedException(GetType().Name, message.ToString());
      }
    }
  }
}
