using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QX.Zoo.Runtime.Scopes;
using QX.Zoo.Runtime.Logging;
using QX.Zoo.Talk.MessageBus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QX.Zoo.Talk.RabbitMQ
{
    delegate void RabbitMQEntityMessageDelegate(string messageId, IDictionary<string, string> headers, object message);

    abstract class RabbitMQEntity : IAsyncBusEntity
    {
        private readonly ILogger _log;
        private readonly Lazy<IModel> _channelLazy;
        private readonly Lazy<EventingBasicConsumer> _consumerLazy;

        public event RabbitMQEntityMessageDelegate MessageReceived;

        public string EntityId { get; }
        internal IModel Channel => _channelLazy.Value;
        protected IOperationScopeProvider OperationScopeProvider { get; }

        protected RabbitMQEntity(IModel channel, string entityId, IOperationScopeProvider operationScopeProvider, ILogger log)
        {
            _log = log;

            EntityId = entityId;
            OperationScopeProvider = operationScopeProvider;
            _consumerLazy = new Lazy<EventingBasicConsumer>(ConfigureConsumer);
            _channelLazy = new Lazy<IModel>(() =>
            {
                ConfigureEntity(channel);
                return channel;
            });
        }

        public void Initalize()
        {
            GC.KeepAlive(_channelLazy.Value);
        }

        public Task<string> PublishMessageAsync<T>(IDictionary<string, string> headers, T message) where T : class
        {
            var id = Guid.NewGuid().ToString("N");
            var props = _channelLazy.Value.CreateBasicProperties();
            props.MessageId = id;
            props.Type = message.GetType().AssemblyQualifiedName;

            SetMessageProperties(props, headers);
            PublishMessage(_channelLazy.Value, props, SerializeMessage(message));

            return Task.FromResult(id);
        }

        public Task<IAsyncBusEntitySubscription> SubscribeAsync(AsyncBusHandlerDelegate asyncHandler)
        {
            return Task.FromResult<IAsyncBusEntitySubscription>(new RabbitMQAsyncSubscription(Subscribe(_consumerLazy.Value), this, asyncHandler, _log));
        }

        protected abstract void PublishMessage(IModel model, IBasicProperties props, byte[] body);
        protected abstract void ConfigureEntity(IModel model);
        protected abstract string Subscribe(IBasicConsumer consumer);

        protected virtual byte[] SerializeMessage<T>(T message) where T : class
        {
            var body = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CumulativeUpdateContractResolver() });
            _log.LogVerbose(body);

            return Encoding.UTF8.GetBytes(body);
        }

        protected virtual object DeserializeMessage(string type, byte[] message)
        {
            var body = Encoding.UTF8.GetString(message);
            _log.LogVerbose(body);

            return MessageDeserializers.Get(type).Deserialize(body);
        }

        protected virtual void SetMessageProperties(IBasicProperties props, IDictionary<string, string> headers)
        {
            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            foreach (var hdr in headers)
            {
                props.Headers[hdr.Key] = hdr.Value;
            }

            props.Persistent = true;
            props.ContentEncoding = "application/json";
        }

        private EventingBasicConsumer ConfigureConsumer()
        {
            var consumer = new EventingBasicConsumer(_channelLazy.Value);
            consumer.Received += ConsumerOnReceived;

            return consumer;
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            _log.LogVerbose($"Consumer at '{EntityId}' entity has received message '{basicDeliverEventArgs.BasicProperties.MessageId}' of type '{basicDeliverEventArgs.BasicProperties.Type}'");

            if (MessageReceived != null)
            {
                var message = DeserializeMessage(basicDeliverEventArgs.BasicProperties.Type, basicDeliverEventArgs.Body);
                var headers = basicDeliverEventArgs.BasicProperties.Headers.ToDictionary(x => x.Key, x => x.Value?.ToString());

                MessageReceived(basicDeliverEventArgs.BasicProperties.MessageId, headers, message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} '{EntityId}'";
        }
    }
}
