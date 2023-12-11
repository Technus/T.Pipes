using System;
using System.Text;
using H.Formatters;
using Newtonsoft.Json;

namespace T.Pipes
{
  /// <summary>
  /// Basic formatter
  /// </summary>
  public class Formatter : FormatterBase
  {
    /// <summary>
    /// Default: UTF8.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// The settings to use
    /// </summary>
    public JsonSerializerSettings Settings { get; set; } = new()
    {
      TypeNameHandling = TypeNameHandling.All,
      Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.CrossAppDomain),
    };

    /// <inheritdoc/>
    protected override byte[] SerializeInternal(object obj)
    {
      var json = JsonConvert.SerializeObject(obj, Settings);
      var bytes = Encoding.GetBytes(json);

      return bytes;
    }

    /// <inheritdoc/>
    protected override T DeserializeInternal<T>(byte[] bytes)
    {
      var json = Encoding.GetString(bytes);
      var obj =
          JsonConvert.DeserializeObject<T>(json, Settings) ??
          throw new InvalidOperationException("Deserialized object is null.");

      return obj;
    }
  }
}
