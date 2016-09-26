using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;
using QX.Zoo.Talk.MessageBus;
using RabbitMQ.Client;

namespace QX.Zoo.Talk.RabbitMQ
{
    public sealed class RabbitMQBroker : IAsyncBusBroker
    {
        private readonly ILogger _log;
        private readonly IOperationScopeProvider _operationScopeProvider;
        private readonly RabbitMQConfiguration _config;
        private readonly Lazy<IConnection> _lazyConnection;

        public RabbitMQBroker(RabbitMQConfiguration config, IOperationScopeProvider operationScopeProvider, ILogger log)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (operationScopeProvider == null) throw new ArgumentNullException(nameof(operationScopeProvider));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _config = config;
            _log = log;
            _operationScopeProvider = operationScopeProvider;

            _lazyConnection = new Lazy<IConnection>(CreateConnection);
        }

        public IAsyncBusEntity GetEntity(string entityId)
        {
            entityId = entityId.Trim('/', ' ');

            if (entityId.StartsWith("private/", StringComparison.CurrentCultureIgnoreCase))
            {
                return new RabbitMQExclusiveQueue(CreateModel(), _operationScopeProvider, _log);
            }

            if (entityId.StartsWith("exchange/", StringComparison.CurrentCultureIgnoreCase))
            {
                return new RabbitMQExchange(CreateModel(), entityId.Replace("exchange/", ""), "fanout", _operationScopeProvider, _log);
            }

            if (entityId.StartsWith("rpc/", StringComparison.CurrentCultureIgnoreCase))
            {
                return new RabbitMQExchange(CreateModel(), entityId.Replace("rpc/", ""), "fanout", _operationScopeProvider, _log);
            }

            return new RabbitMQQueue(CreateModel(), entityId, _operationScopeProvider, _log);
        }

        private IConnection CreateConnection()
        {
            return new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                Protocol = Protocols.AMQP_0_9_1,
                Uri = string.Join(";", _config.HostNames.Select(host => $"amqp://{_config.UserName}:{_config.Password}@{host}:{_config.Port}{_config.VirtualHost}"))
            }.CreateConnection();
        }

        private IModel CreateModel()
        {
            return _lazyConnection.Value.CreateModel();
        }
    }
}
