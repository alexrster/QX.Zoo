using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace QX.Zoo.Hold
{
  public abstract class DocumentStore<TDocument, TKey> where TDocument : StoredDocument
  {
    private readonly Func<TDocument, TKey> _keyFunc;
    private readonly Expression<Func<TDocument, TKey>> _storeDocumentKeyExpression;
    private readonly ILogger _log;

    protected DocumentStore(Expression<Func<TDocument, TKey>> keyExpression, ILogger log)
    {
      if (keyExpression.Parameters.Count > 1)
      {
        throw new NotSupportedException("Expressions with more than 1 parameter not supported");
      }

      var memberExpression = keyExpression.Body as MemberExpression;
      if (memberExpression == null)
      {
        throw new NotSupportedException();
      }

      _storeDocumentKeyExpression = keyExpression;
      _log = log;
      _keyFunc = keyExpression.Compile();
    }

    public async Task<TDocument> GetAsync(Guid recordId)
    {
      return await GetDocumentAsync(recordId);
    }

    public async Task<KeyValuePair<Guid, TDocument>> GetAsync(TKey key)
    {
      var doc = await GetDocumentAsync(key);
      return doc != null ? new KeyValuePair<Guid, TDocument>(doc.RecordId, doc) : new KeyValuePair<Guid, TDocument>(Guid.Empty, null);
    }

    public async Task<IEnumerable<KeyValuePair<Guid, TDocument>>> FindAsync(Expression<Func<TDocument, bool>> filter)
    {
      return (await FindDocumentAsync(filter)).Select(x => new KeyValuePair<Guid, TDocument>(x.RecordId, x));
    }

    public Task<Guid> CreateAsync(TDocument document)
    {
      return UpdateAsync(new Guid(NewId()), document);
    }

    public async Task<Guid> UpdateAsync(TDocument document)
    {
      var storedDocument = await GetDocumentAsync(_keyFunc(document));
      return await UpdateAsync(IncrementDocumentVersion(storedDocument.RecordId), document);
    }

    public async Task<Guid> UpdateAsync(Guid documentRecordId, TDocument document)
    {
      document.RecordId = IncrementDocumentVersion(documentRecordId);
      await PutDocumentAsync(document);

      return document.RecordId;
    }

    protected abstract Task<IEnumerable<TDocument>> FindDocumentAsync(Expression<Func<TDocument, bool>> filter);
    protected abstract Task<TDocument> GetDocumentAsync(Guid recordId);
    protected abstract Task<TDocument>GetDocumentAsync(TKey key);
    protected abstract Task PutDocumentAsync(TDocument document);

    protected Expression<Func<TDocument, bool>> GetFilterExpressionWithKey(TKey key)
    {
      return Expression.Lambda<Func<TDocument, bool>>(Expression.Equal(_storeDocumentKeyExpression.Body, Expression.Constant(key)), _storeDocumentKeyExpression.Parameters[0]);
    }

    private Guid IncrementDocumentVersion(Guid recordId)
    {
      var bytes = recordId != Guid.Empty ? recordId.ToByteArray() : NewId();

      Array.Copy(BitConverter.GetBytes(BitConverter.ToInt32(bytes, 12) + 1), 0, bytes, 12, 4);
      var result = new Guid(bytes);

      _log.LogTrace("Generate new version '{0}'", result);
      return result;
    }

    private static byte[] NewId()
    {
      var bytes = Guid.NewGuid().ToByteArray();
      Array.Clear(bytes, 12, 4);

      return bytes;
    }
  }
}
