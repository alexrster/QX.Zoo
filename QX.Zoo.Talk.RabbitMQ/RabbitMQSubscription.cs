using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Talk.RabbitMQ
{
    abstract class RabbitMQSubscription : IAsyncBusEntitySubscription
    {
        public static class Events
        {
            public static EventId Created = 11001;
            public static EventId Canceled = 11002;
            public static EventId HandlerError = 11003;
            public static EventId HandlerCanceled = 11004;
            public static EventId HandlerSuccess = 11005;
        }

        private readonly ILogger _log;
        private readonly RabbitMQEntity _entity;

        public string SubscriptionId { get; }
        public string EntityId => _entity.EntityId;

        protected RabbitMQSubscription(string subscriptionId, RabbitMQEntity entity, ILogger log)
        {
            _entity = entity;
            _log = log;
            SubscriptionId = subscriptionId;

            _entity.MessageReceived += OnEntityMessageReceived;

            _log.LogInformation(Events.Created, $"New RabbitMQ subscription '{SubscriptionId}' for entity '{EntityId}'");
        }

        public Task CancelAsync()
        {
            _log.LogInformation(Events.Canceled, $"Cancel RabbitMQ subscription '{SubscriptionId}' for entity '{EntityId}'");

            _entity.MessageReceived -= OnEntityMessageReceived;
            return Task.FromResult(0);
        }

        protected abstract Task ExecuteHandler(IAsyncBusEntity entity, string messageId, IDictionary<string, string> headers, object message);

        private void OnEntityMessageReceived(string messageId, IDictionary<string, string> headers, object message)
        {
            Task.Run(async () => await ExecuteHandler(_entity, messageId, headers, message))
                .ContinueWith(OnEntityMessageHandlerCompleted);
        }

        private void OnEntityMessageHandlerCompleted(Task task)
        {
            if (task.IsFaulted)
            {
                _log.LogError(Events.HandlerError, task.Exception, $"Subscription '{SubscriptionId}' handler failed to process message");
            }
            else if (task.IsCanceled)
            {
                _log.LogWarning(Events.HandlerCanceled, $"Subscription '{SubscriptionId}' handler has been canceled during processing message");
            }
            else
            {
                _log.LogDebug(Events.HandlerSuccess, $"Subscription '{SubscriptionId}' handler has been successfully completed");
            }
        }
    }
}
