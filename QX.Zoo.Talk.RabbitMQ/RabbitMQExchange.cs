using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using RabbitMQ.Client;

namespace QX.Zoo.Talk.RabbitMQ
{
  class RabbitMQExchange : RabbitMQEntity
  {
    private readonly string _exchangeType;
    private readonly ILogger _log;

    public RabbitMQExchange(IModel channel, string entityId, string exchangeType, IOperationScopeProvider operationScopeProvider, ILogger log) 
      : base(channel, entityId, operationScopeProvider, log)
    {
      _exchangeType = exchangeType;
      _log = log;
    }

    protected override void PublishMessage(IModel model, IBasicProperties props, byte[] body)
    {
      model.BasicPublish(EntityId, "", props, body);
    }

    protected override void ConfigureEntity(IModel model)
    {
      model.ExchangeDeclare(EntityId, _exchangeType);
    }

    protected override string Subscribe(IBasicConsumer consumer)
    {
      var queue = new RabbitMQExclusiveQueue(consumer.Model, OperationScopeProvider, _log);
      queue.Initalize();

      consumer.Model.QueueBind(queue.EntityId, EntityId, "");

      return consumer.Model.BasicConsume(queue.EntityId, true, consumer);
    }
  }
}
