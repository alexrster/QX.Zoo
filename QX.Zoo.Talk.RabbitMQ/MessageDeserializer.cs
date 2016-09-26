using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QX.Zoo.Talk.RabbitMQ
{
  class MessageDeserializer<T> : MessageDeserializer where T : class
  {
    public override object Deserialize(string message)
    {
      return JsonConvert.DeserializeObject<T>(message, new JsonSerializerSettings { ContractResolver = new CumulativeUpdateContractResolver() });
    }
  }

  abstract class MessageDeserializer
  {
    public abstract object Deserialize(string message);
  }

  class CumulativeUpdateContractResolver : DefaultContractResolver
  {
    private readonly Type _cumulativeUpdateType;
    private readonly Lazy<CumulativeUpdateConverter> _converterLazy = new Lazy<CumulativeUpdateConverter>(); 

    public CumulativeUpdateContractResolver()
    { }

    public CumulativeUpdateContractResolver(Type cumulativeUpdateType)
    {
      _cumulativeUpdateType = cumulativeUpdateType;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      if (type.IsConstructedGenericType && type.FullName.Contains("VersionCumulativeUpdateNotification"))
      {
        var props = base.CreateProperties(type, memberSerialization);
        props.First(x => x.PropertyName == "CumulativeUpdate").Converter = new CumulativeUpdateConverter();
        
        return props;
      }

      return base.CreateProperties(type, memberSerialization);
    }

    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
      if (objectType.FullName.Contains("ICumulativeUpdate"))
      {
        return _converterLazy.Value;
      }
      if (objectType.FullName.Contains("VersionCumulativeUpdateNotification"))
      {
        return _converterLazy.Value;
      }

      return base.ResolveContractConverter(objectType);
    }
  }

  class CumulativeUpdateConverter : JsonConverter
  {
    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value != null)
      {
        if (!value.GetType().FullName.Contains("VersionCumulativeUpdateNotification"))
        {
          serializer.Serialize(writer, value, value.GetType());
          writer.WritePropertyName("CumulativeUpdateType");
          writer.WriteValue(value.GetType().FullName);
        }
      }
      else
      {
        serializer.Serialize(writer, null);
      }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      return serializer.Deserialize(reader);
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType.FullName.Contains("ICumulativeUpdate") || objectType.FullName.Contains("VersionCumulativeUpdateNotification");
    }
  }
}
