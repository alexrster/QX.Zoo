using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.FabricHost.Infrastructure
{
    class AsyncBusListenersFactory
    {
        public ServiceInstanceListener ActionsListener { get; }

        public AsyncBusListenersFactory(IAsyncBusBroker asyncBusBroker, string actionsEntityId, ILogger log)
        {
            var entity = asyncBusBroker.GetEntity(actionsEntityId);
            ActionsListener = new ServiceInstanceListener(ctx => new AsyncBusListener(entity, ctx, log), entity.EntityId);
        }
    }
}
