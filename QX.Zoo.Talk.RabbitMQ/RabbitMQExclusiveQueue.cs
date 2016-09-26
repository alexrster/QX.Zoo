using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using RabbitMQ.Client;

namespace QX.Zoo.Talk.RabbitMQ
{
  class RabbitMQExclusiveQueue : RabbitMQQueue
  {
    private readonly IDictionary<string, object> _arguments;

    public RabbitMQExclusiveQueue(IModel channel, IOperationScopeProvider operationScopeProvider, ILogger log, bool noAck = true, IDictionary<string, object> arguments = null)
      : base(channel, $"private-{Guid.NewGuid().ToString("N")}", operationScopeProvider, log, noAck)
    {
      _arguments = arguments;
    }

    protected override void ConfigureEntity(IModel model)
    {
      model.QueueDeclare(EntityId, true, true, true, _arguments);
      model.BasicQos(0, 1, false);
    }
  }
}
