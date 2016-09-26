using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.FabricHost.Infrastructure
{
    class AsyncBusListener : ICommunicationListener
    {
        private readonly IAsyncBusEntity _asyncBusEntity;
        private readonly StatelessServiceContext _serviceContext;
        private readonly ILogger _log;
        private IAsyncBusEntitySubscription _asyncBusEntitySubscription = null;

        public AsyncBusListener(IAsyncBusEntity asyncBusEntity, StatelessServiceContext serviceContext, ILogger log)
        {
            _asyncBusEntity = asyncBusEntity;
            _serviceContext = serviceContext;
            _log = log;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation($"Open Fabric Async Bus Listener for '{_asyncBusEntity.EntityId}'");

            _asyncBusEntitySubscription = await _asyncBusEntity.SubscribeAsync(OnMessage);
            return _asyncBusEntitySubscription.SubscriptionId;
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation($"Close Fabric Async Bus Listener for '{_asyncBusEntity.EntityId}'");

            if (_asyncBusEntitySubscription != null)
            {
                _log.LogTrace($"Close Fabric Async Bus Listener Subscription '{_asyncBusEntitySubscription.SubscriptionId}' for '{_asyncBusEntity.EntityId}'");
                return _asyncBusEntitySubscription.CancelAsync();
            }

            return Task.FromResult(0);
        }

        public void Abort()
        {
            _log.LogInformation($"Abort Fabric Async Bus Listener for '{_asyncBusEntity.EntityId}'");
            CloseAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(5));
        }

        protected Task OnMessage(IAsyncBusEntity entity, string messageId, IDictionary<string, string> headers, object data)
        {
            _log.LogTrace($"Fabric Async Bus Listener received message '{messageId}' from '{entity.EntityId}': {data}");

            // TBD

            return Task.FromResult(0);
        }
    }
}
