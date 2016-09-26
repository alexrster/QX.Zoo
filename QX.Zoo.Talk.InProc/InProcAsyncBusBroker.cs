using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using QX.Zoo.Runtime.Logging;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Talk.InProc
{
  public sealed class InProcAsyncBusBroker : IAsyncBusBroker, IDisposable
  {
    private readonly ILogger _log;
    private readonly IOperationScopeProvider _operationScopeProvider;
    private readonly ConcurrentDictionary<string, InProcAsyncBusEntity> _entities = new ConcurrentDictionary<string, InProcAsyncBusEntity>();
    private readonly ManualResetEvent _reset = new ManualResetEvent(false);

    public InProcAsyncBusBroker(ILogger log, IOperationScopeProvider operationScopeProvider)
    {
      _log = log;
      _operationScopeProvider = operationScopeProvider;

      ThreadPool.QueueUserWorkItem(ProcessMessages);
    }

    public IAsyncBusEntity GetEntity(string entityId)
    {
      _log.LogVerbose($"Get async bus entity '{entityId}'");
      return _entities.GetOrAdd(entityId, CreateEntity);
    }

    private InProcAsyncBusEntity CreateEntity(string entityId)
    {
      entityId = entityId.Trim('/', ' ');
      _log.LogInformation($"Create new async bus entity '{entityId}'");

      if (entityId.StartsWith("exchange/"))
      {
        return new InProcAsyncBusTopic(entityId.Replace("exchange/", ""), _log, _operationScopeProvider);
      }

      return new InProcAsyncBusQueue(entityId, _log, _operationScopeProvider);
    }

    private void ProcessMessages(object state)
    {
      _log.LogInformation("Started InProc async bus broker worker");
      while (!_reset.WaitOne(10))
      {
        foreach (var queue in _entities.Values)
        {
          var processTask = queue.ProcessNextMessage();
          if (processTask == null)
          {
            continue;
          }

          _log.LogVerbose("Start processing next message using task #'{0}'", processTask.Id);

          var scope = _log.BeginScope(queue.EntityId);
          Task.Run(async () => await processTask).ContinueWith(t =>
          {
            _log.LogVerbose("End processing message by task #'{0}' - {1:F}, exception: {2}", t.Id, t.Status, t.Exception?.ToString() ?? "<NULL>");
            scope.Dispose();
          });
        }
      }

      _log.LogWarning("Stopped InProc async bus broker worker");
    }

    public void Dispose()
    {
      if (_reset.Set())
      {
        _reset.Dispose();
      }
    }
  }
}
