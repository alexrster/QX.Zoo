using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QX.Zoo.Runtime.Logging;

namespace QX.Zoo.Hold.MongoDB
{
  public class MongoDocumentStore<TDocument, TKey> : DocumentStore<TDocument, TKey> where TDocument : StoredDocument
  {
    private readonly ILogger _log;

    protected IMongoCollection<TDocument> Collection { get; }

    private MongoDocumentStore(Expression<Func<TDocument, TKey>> keyExpression, IMongoCollection<TDocument> collection, ILogger log) : base(keyExpression, log)
    {
      _log = log;
      Collection = collection;
    }

    public static MongoDocumentStore<TDocument, TKey> Create(Expression<Func<TDocument, TKey>> keyExpression, string mongoDatabaseUrl, string collectionName, ILogger log)
    {
      log.LogInformation("Create MongoDocumentStore '{0}' with primary key property '{1}' primary key for MongoDB collection at '{2}/{3}'", typeof(MongoDocumentStore<TDocument, TKey>).Name, keyExpression, mongoDatabaseUrl, collectionName);
      return new MongoDocumentStore<TDocument, TKey>(
        keyExpression, 
        new MongoClient(mongoDatabaseUrl).GetDatabase(new MongoUrlBuilder(mongoDatabaseUrl).DatabaseName).GetCollection<TDocument>(collectionName),
        log);
    }

    protected override async Task<IEnumerable<TDocument>> FindDocumentAsync(Expression<Func<TDocument, bool>> filter)
    {
      _log.LogVerbose("Find document with filter '{0}'", filter);
      return await Collection.Aggregate().Match(filter).ToListAsync();
    }

    protected override async Task<TDocument> GetDocumentAsync(Guid recordId)
    {
      _log.LogVerbose("Get document by record id '{0}'", recordId);
      return await Collection.FindSync(x => x.RecordId == recordId).FirstOrDefaultAsync();
    }

    protected override async Task<TDocument> GetDocumentAsync(TKey key)
    {
      _log.LogVerbose("Get latest document by key id '{0}'", key);
      return await Collection.FindSync(GetFilterExpressionWithKey(key), new FindOptions<TDocument> { Sort = Builders<TDocument>.Sort.Descending("_id")}).FirstOrDefaultAsync();
    }

    protected override Task PutDocumentAsync(TDocument document)
    {
      _log.LogVerbose("Put document with record id: '{1}': '{0}'", document, document.RecordId);
      return Collection.InsertOneAsync(document);
    }
  }
}
