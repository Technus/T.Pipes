using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Abstractions
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
  public abstract partial class BaseClass
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
  {
    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T GetPropertyValue<T>(T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property ?? throw new ArgumentNullException(propertyName);
    }

    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T GetReadonlyProperty<T>(ref readonly T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property ?? throw new ArgumentNullException(propertyName);
    }

    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T GetProperty<T>(ref T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property ?? throw new ArgumentNullException(propertyName);
    }

    /// <summary>
    /// Safety wrapper for property set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    protected virtual void SetProperty<T>(ref T? property, T value, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      property = value ?? throw new ArgumentNullException(propertyName);
    }

    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T? GetOptionalPropertyValue<T>(T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property;
    }

    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T? GetOptionalReadonlyProperty<T>(ref readonly T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property;
    }

    /// <summary>
    /// Safety wrapper for property get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T? GetOptionalProperty<T>(ref T? property, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      return property;
    }

    /// <summary>
    /// Safety wrapper for property set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    protected virtual void SetOptionalProperty<T>(ref T? property, T? value, [CallerMemberName] string? propertyName = null)
    {
      ThrowIfDisposed();
      property = value;
    }
  }
}
