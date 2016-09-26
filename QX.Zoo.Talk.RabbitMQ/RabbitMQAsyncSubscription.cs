using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Talk.RabbitMQ
{
    class RabbitMQAsyncSubscription : RabbitMQSubscription
    {
        private readonly AsyncBusHandlerDelegate _asyncHandler;

        public RabbitMQAsyncSubscription(string subscriptionId, RabbitMQEntity entity, AsyncBusHandlerDelegate asyncHandler, ILogger log) 
            : base(subscriptionId, entity, log)
        {
            _asyncHandler = asyncHandler;
        }

        protected override Task ExecuteHandler(IAsyncBusEntity entity, string messageId, IDictionary<string, string> headers, object message)
        {
            return _asyncHandler(entity, messageId, headers, message);
        }
    }
}