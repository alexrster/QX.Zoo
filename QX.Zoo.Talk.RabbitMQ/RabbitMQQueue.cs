using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using RabbitMQ.Client;

namespace QX.Zoo.Talk.RabbitMQ
{
    class RabbitMQQueue : RabbitMQEntity
    {
        private readonly bool _noAck;

        public RabbitMQQueue(IModel channel, string entityId, IOperationScopeProvider operationScopeProvider, ILogger log, bool noAck = true)
          : base(channel, entityId, operationScopeProvider, log)
        {
            _noAck = noAck;
        }

        protected override void PublishMessage(IModel model, IBasicProperties props, byte[] body)
        {
            model.BasicPublish(string.Empty, EntityId, props, body);
        }

        protected override void ConfigureEntity(IModel model)
        {
            model.QueueDeclarePassive(EntityId);
            model.BasicQos(0, 1, false);
        }

        protected override string Subscribe(IBasicConsumer consumer)
        {
            return consumer.Model.BasicConsume(EntityId, _noAck, consumer);
        }
    }
}
