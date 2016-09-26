using System;
using System.Collections.Concurrent;

namespace QX.Zoo.Talk.RabbitMQ
{
  static class MessageDeserializers
  {
    private static readonly ConcurrentDictionary<string, MessageDeserializer> Deserializers = new ConcurrentDictionary<string, MessageDeserializer>();

    public static MessageDeserializer Get(string messageType)
    {
      return Deserializers.GetOrAdd(messageType, k => (MessageDeserializer) Activator.CreateInstance(CreateGenericDeserializerType(messageType)));
    }

    private static Type CreateGenericDeserializerType(string messageType)
    {
      var innerType = Type.GetType(messageType);
      var deserializerType = typeof (MessageDeserializer<>).MakeGenericType(innerType);

      return deserializerType;
    }
  }
}
