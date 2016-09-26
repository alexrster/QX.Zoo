using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QX.Zoo.Accumulative;
using QX.Zoo.Talk.MessageBus;
using QX.Zoo.Tests.Animals;
using QX.Zoo.Runtime.Accumulative;
using QX.Zoo.Runtime.Accumulative.Actors;
using QX.Zoo.Runtime.Toys;
using QX.Zoo.Talk.RabbitMQ;

namespace QX.Zoo.Tests
{
    public class UnitTest1
    {
        private IAsyncBusBroker _busBroker;
        private IAccumulatingFactory<Citizen> _instance1;
        private IAccumulatingFactory<Citizen> _instance2;
        private IAccumulatingFactory<Citizen> _instance3;

        public ILoggerFactory LoggerFactory { get; set; }
        public IConfigurationRoot Configuration { get; set; }

        private async Task Setup()
        {
            var rootOperationScopeProvider = new OperationScopeProvider();

            //_busBroker = new InProcAsyncBusBroker(LoggerFactory.CreateLogger("QX.Zoo.InProcAsyncBus"), rootOperationScopeProvider);
            _busBroker = new RabbitMQBroker(Configuration.GetValue<RabbitMQConfiguration>("rabbitmq"), rootOperationScopeProvider, LoggerFactory.CreateLogger("QX.Zoo.InProcAsyncBus"));

            var factoryId = SequentialGuidGenerator.GetOrCreateGenerator<Citizen>().NewGuid();
            var maxVersionNumber = StaticCitizensWarehouse.Version;
            var warehouse = new WarehouseActor<Citizen>(StaticCitizensWarehouse.ExternalLoader, LoggerFactory.CreateLogger("QX.Zoo.Warehouse"));

            _instance1 = new AccumulatingFactory<Citizen>(factoryId, 1, maxVersionNumber, b => b.GetEntity("exchange/zoo"), x => LoggerFactory.CreateLogger(x?.ToString()));
            _instance2 = new AccumulatingFactory<Citizen>(factoryId, 2, maxVersionNumber, b => b.GetEntity("exchange/zoo"), x => LoggerFactory.CreateLogger(x?.ToString()));
            _instance3 = new AccumulatingFactory<Citizen>(factoryId, 3, maxVersionNumber, b => b.GetEntity("exchange/zoo"), x => LoggerFactory.CreateLogger(x?.ToString()));

            await _instance1.StartAsync(_busBroker);
            await _instance2.StartAsync(_busBroker);
            await _instance3.StartAsync(_busBroker);
        }

        //[Fact(DisplayName = "Citizen can born")]
        public async Task CitizenCanBeBorn()
        {
            await Setup();

            var version = await _instance1.ApplyUpdateAsync(new CreateCitizen { Address = "Moon", CitizenId = "CTZZZ01" });
            //Assert.NotEqual(0, version);
        }

        //[Fact(DisplayName = "Citizen can be moved")]
        public async Task CitizenCanBeMoved()
        {
            await Setup();

            var version3 = await _instance1.ApplyUpdateAsync(new MoveCitizen { BaseVersionNumber = 1, NewAddress = "Tres address" });
            //Assert.Equal(3, version3);

            var version4 = await _instance2.ApplyUpdateAsync(new MoveCitizen { BaseVersionNumber = 1, NewAddress = "Tres address" });
            //Assert.Equal(4, version4);
        }
    }

    public sealed class StaticCitizensWarehouse
    {
        private static readonly IDictionary<long, Citizen> CitizensData = new Dictionary<long, Citizen>()
        {
            {1, new Citizen {CitizenId = "Uno", Address = "Uno address"}},
            {2, new Citizen {CitizenId = "Duo", Address = "Duo address"}}
        };

        public static long Version => CitizensData.Keys.Max();

        public static Task<Citizen> ExternalLoader(long index)
        {
            return Task.FromResult(CitizensData[index]);
        }
    }
}
