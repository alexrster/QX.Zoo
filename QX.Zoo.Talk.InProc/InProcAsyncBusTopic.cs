using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Talk.InProc
{
  class InProcAsyncBusTopic : InProcAsyncBusEntity
  {
    public InProcAsyncBusTopic(string entityId, ILogger log, IOperationScopeProvider operationScopeProvider) : base(entityId, operationScopeProvider, log)
    { }

    protected override Task ProcessMessage(string messageId, IDictionary<string, string> headers, object message)
    {
      var tasks = new List<Task>();
      foreach (var handler in Subscribers)
      {
        try
        {
          var task = handler.Value?.Invoke(this, messageId, headers, message);
          if (task != null)
          {
            //Trace.TraceInformation($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' handling started by subscriber with ID '{handler.Key}' on entity '{EntityId}'");
            tasks.Add(task);
          }
          else
          {
            //Trace.TaceVerbose($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' cannot be handled by subscriber with ID '{handler.Key}' on entity '{EntityId}'");
          }
        }
        catch (Exception e)
        {
          //Trace.TraceWarning($"Got exception while trying to handle message '{messageId}' by subscriber with ID '{handler.Key}' on entity '{EntityId}' - {e}");
        }
      }

      if (tasks.Count == 0)
      { 
        //Trace.TraceWarning($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' cannot be handled by any subscribers currently registered on entity '{EntityId}'");
        return null;
      }

      //Trace.TraceInformation($"Message '{messageId}' of type '{message?.GetType().Name ?? "<NULL>"}' at entity '{EntityId}' - wait for {tasks.Count} handler(s) to complete");
      return Task.WhenAll(tasks);
    }
  }
}
