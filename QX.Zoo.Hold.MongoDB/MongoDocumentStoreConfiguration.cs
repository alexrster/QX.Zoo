using System;
using MongoDB.Bson.Serialization;

namespace QX.Zoo.Hold.MongoDB
{
  public static class MongoDocumentStoreConfiguration
  {
    public static void ConfigureStoredDocumentMappings<TDocument>(Action<BsonClassMap<TDocument>> config = null) where TDocument : StoredDocument
    {
      BsonClassMap.RegisterClassMap<TDocument>(cm =>
      {
        (config ?? (m => m.AutoMap())).Invoke(cm);
      });
    }
  }
}
