using System;
using System.Globalization;
using System.Runtime.Serialization;
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
    /// Boxing of C# primitives for serialization
    /// </summary>
    public class PrimitiveConverter : JsonConverter
    {
      /// <inheritdoc/>
      public override bool CanConvert(Type objectType) => true;

      /// <inheritdoc/>
      public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        => serializer.Deserialize(reader, objectType);

      /// <inheritdoc/>
      public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
      {
        if (value is not null && value.GetType() is var type && type.IsPrimitive)
        {
          writer.WriteStartObject();
          writer.WritePropertyName("$type");
          writer.WriteValue($"{type.FullName}, {type.Assembly.GetName().Name}");
          writer.WritePropertyName("$value");
          writer.WriteValue(Convert.ToString(value, CultureInfo.InvariantCulture));
          writer.WriteEndObject();
        }
        else serializer.Serialize(writer, value);
      }
    }

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
      TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
      ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
#pragma warning disable SYSLIB0050 // Type or member is obsolete
      Context = new StreamingContext(StreamingContextStates.CrossAppDomain),
#pragma warning restore SYSLIB0050 // Type or member is obsolete
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
