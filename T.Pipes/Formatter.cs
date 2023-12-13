using System;
using System.Globalization;
using System.Text;
using H.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace T.Pipes
{
  /// <summary>
  /// Basic formatter
  /// </summary>
  public class Formatter : FormatterBase
  {
    public class PrimitiveConverter : JsonConverter
    {
      public override bool CanConvert(Type objectType) => true;

      public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        => serializer.Deserialize(reader, objectType);

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
