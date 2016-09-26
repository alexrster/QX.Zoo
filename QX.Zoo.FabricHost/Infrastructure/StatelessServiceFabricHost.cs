using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using QX.Zoo.Runtime.Accumulative;
using QX.Zoo.Runtime.Toys;
using QX.Zoo.Talk.MessageBus;
using QX.Zoo.Talk.RabbitMQ;
using QX.Zoo.Tests;
using QX.Zoo.Tests.Animals;

namespace QX.Zoo.FabricHost.Infrastructure
{
    class StatelessServiceFabricHost : StatelessService
    {
        private readonly StatelessServiceContext _serviceContext;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;
        private readonly RabbitMQBroker _asyncBusBroker;
        private readonly AccumulatingFactory<Citizen> _instance;
        private readonly ConfigurationSettings _config;
        private IAsyncBusEntity _actionsEntity;
        private IAsyncBusEntitySubscription _actionsSubscription;

        public StatelessServiceFabricHost(StatelessServiceContext serviceContext) : base(serviceContext)
        {
            _serviceContext = serviceContext;
            _loggerFactory = new LoggerFactory().AddConsole();

            _log = _loggerFactory.CreateLogger("StatelessService");
            _log.LogTrace("StatelessServiceFabricHost .ctor");

            _config = serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;
            if (_config == null) throw new ArgumentException("Configuration package 'Config' not found");

            var rmqConfig = _config.Sections["RabbitMQ"];
            if (rmqConfig == null) throw new ArgumentException("Configuration section 'RabbitMQ' not found");

            var rabbitConfig = new RabbitMQConfiguration
            {
                HostNames = rmqConfig.Parameters["hostnames"]?.Value?.Split(';').ToList(),
                UserName = rmqConfig.Parameters["userName"]?.Value,
                Password = rmqConfig.Parameters["password"]?.Value,
                Port = int.Parse(rmqConfig.Parameters["port"]?.Value ?? "5672")
            };

            _asyncBusBroker = new RabbitMQBroker(rabbitConfig, new OperationScopeProvider(), _loggerFactory.CreateLogger("RabbitMQ"));

            //var warehouse = new WarehouseActor<Citizen>(StaticCitizensWarehouse.ExternalLoader, _loggerFactory.CreateLogger("Warehouse"));
            _instance = new AccumulatingFactory<Citizen>(
                SequentialGuidGenerator.GetOrCreateGenerator<Citizen>().NewGuid(),
                1,
                StaticCitizensWarehouse.Version,
                b => b.GetEntity("exchange/zoo"),
                x => _loggerFactory.CreateLogger(x?.ToString()));
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            _log.LogInformation("Create Service Listeners");

            var asyncBusConfig = _config.Sections["AsyncBus"];
            if (asyncBusConfig == null) throw new ArgumentException("Configuration section 'AsyncBus' not found");

            _actionsEntity = _asyncBusBroker.GetEntity(asyncBusConfig.Parameters["ActionsEntity"].Value);
            return new[]
            {
                new ServiceInstanceListener(ctx => new AsyncBusListener(_actionsEntity, ctx, _loggerFactory.CreateLogger("ActionsEntityListener")), _actionsEntity.EntityId)
            };
        }

        private Task OnActionReceived(IAsyncBusEntity entity, string messageId, IDictionary<string, string> headers, object data)
        {
            _log.LogInformation($"Received message '{messageId}' from '{entity.EntityId}' with Action");
            return Task.FromResult(0);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Start");
            await _instance.StartAsync(_asyncBusBroker);

            _actionsSubscription = await _actionsEntity.SubscribeAsync(OnActionReceived);
        }
    }
}
