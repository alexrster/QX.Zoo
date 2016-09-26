using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using QX.Zoo.Talk.MessageBus;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using QX.Zoo.Runtime.Logging;

namespace QX.Zoo.Talk.InProc
{
  abstract class InProcAsyncBusEntity : IAsyncBusEntity
  {
    private readonly ILogger _log;
    private readonly IOperationScopeProvider _operationScopeProvider;
    private readonly ConcurrentDictionary<string, AsyncBusHandlerDelegate> _subscribers = new ConcurrentDictionary<string, AsyncBusHandlerDelegate>();
    private readonly ConcurrentQueue<KeyValuePair<string, Tuple<object, IDictionary<string, string>>>> _messagesToSend = new ConcurrentQueue<KeyValuePair<string, Tuple<object, IDictionary<string, string>>>>();

    public string EntityId { get; }

    protected IEnumerable<KeyValuePair<string, AsyncBusHandlerDelegate>> Subscribers => _subscribers.ToArray();

    protected InProcAsyncBusEntity(string entityId, IOperationScopeProvider operationScopeProvider, ILogger log)
    {
      EntityId = entityId;
      _log = log;
      _operationScopeProvider = operationScopeProvider;
    }

    public Task<string> PublishMessageAsync<T>(IDictionary<string, string> headers, T message) where T : class
    {
      var messageId = Guid.NewGuid().ToString("D");
      _log.LogVerbose($"Publish message '{messageId}' to InProcAsyncBus entity '{EntityId}'");

      headers = headers ?? new Dictionary<string, string>();
      headers["x-operation-scope"] = _operationScopeProvider.Current?.ToString();

      _messagesToSend.Enqueue(
        new KeyValuePair<string, Tuple<object, IDictionary<string, string>>>(
          messageId, 
          new Tuple<object, IDictionary<string, string>>(
            message, 
            headers)));

      return Task.FromResult(messageId);
    }

    public Task<IAsyncBusEntitySubscription> SubscribeAsync(AsyncBusHandlerDelegate asyncHandler)
    {
      var subscription = new InProcAsyncBusEntitySubscription(EntityId, CancelSubscription, _log);
      _subscribers.TryAdd(subscription.SubscriptionId, asyncHandler);

      //Trace.TraceVerbose($"Created new async bus entity '{EntityId}' subscription - '{subscription.SubscriptionId}'");
      return Task.FromResult<IAsyncBusEntitySubscription>(subscription);
    }

    internal Task ProcessNextMessage()
    {
      KeyValuePair<string, Tuple<object, IDictionary<string, string>>> message;
      if (!_messagesToSend.TryDequeue(out message))
      {
        return null;
      }

      //Trace.TraceVerbose($"Start message '{message.Key}' processing");
      return ProcessMessage(message.Key, message.Value.Item2, message.Value.Item1);
    }

    protected abstract Task ProcessMessage(string messageId, IDictionary<string, string> dictionary, object message);

    private Task CancelSubscription(string subscriptionId)
    {
      //Trace.TraceVerbose($"Cancel async bus entity '{EntityId}' subscription '{subscriptionId}'");

      AsyncBusHandlerDelegate handlerFunc;
      _subscribers.TryRemove(subscriptionId, out handlerFunc);

      return Task.FromResult(0);
    }
  }
}
