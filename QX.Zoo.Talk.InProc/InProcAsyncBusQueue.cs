using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Talk.InProc
{
  class InProcAsyncBusQueue : InProcAsyncBusEntity
  {
    private readonly ILogger _log;

    public InProcAsyncBusQueue(string entityId, ILogger log, IOperationScopeProvider operationScopeProvider) : base(entityId, operationScopeProvider, log)
    {
      _log = log;
    }

    protected override Task ProcessMessage(string messageId, IDictionary<string, string> headers, object message)
    {
      foreach (var handler in Subscribers)
      {
        try
        {
          var task = handler.Value?.Invoke(this, messageId, headers, message);
          if (task != null)
          {
            //Trace.TraceInformation($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' handling started by subscriber with ID '{handler.Key}' on entity '{EntityId}'");
            return task;
          }

          //Trace.TraceVerbose($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' cannot be handled by subscriber with ID '{handler.Key}' on entity '{EntityId}'");
        }
        catch (Exception e)
        {
          _log.LogWarning($"Got exception while trying to handle message '{messageId}' by subscriber with ID '{handler.Key}' on entity '{EntityId}': {e}");
        }
      }

      //Trace.TraceWarning($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' cannot be handled by any subscribers currently registered on entity '{EntityId}'");
      return null;
    }
  }
}
