using System;
using System.Threading.Tasks;
using QX.Zoo.Accumulative;
using QX.Zoo.Hold;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Runtime.Accumulative.Actors
{
    class AttendeeActor<T> : IAccumulatingFactory where T : ICloneable<T>, new()
    {
        private readonly Func<IAsyncBusBroker, IAsyncBusEntity> _busEntityFunc;
        private int _isInitialized;

        public Guid FactoryId { get; }
        public long FactoryInstanceId { get; }

        public AttendeeActor(Guid factoryId, long factoryInstanceId, Func<IAsyncBusBroker, IAsyncBusEntity> busEntityFunc)
        {
            _busEntityFunc = busEntityFunc;
            FactoryId = factoryId;
            FactoryInstanceId = factoryInstanceId;
        }

        public Task StartAsync(IAsyncBusBroker busBroker)
        {
            var busEntity = _busEntityFunc(busBroker);
            _isInitialized = 1;

            return Task.FromResult(0);
        }

        public Task<long> ApplyUpdateAsync(ICumulativeUpdate<T> update)
        {
            throw new NotImplementedException();
            //if (_isInitialized == 0)
            //{
            //  throw new InvalidOperationException($"FrontEnt actor #{FactoryInstanceId} of factory '{FactoryId}' not initialized");
            //}

            //Trace.TraceVerbose($"Vendor actor #{FactoryInstanceId} of factory '{FactoryId}' apply update of type {update.GetType()} for base #{update.BaseVersionNumber}");

            //var snapshot = _librarian.ApplyUpdate(update);
            //Trace.TraceVerbose($"Vendor actor #{FactoryInstanceId} of factory '{FactoryId}' generated new verion #{snapshot.VersionNumber} for base #{update.BaseVersionNumber} using internal Vendor actor");

            //VersionAccumulator current;
            //_versions.Update(
            //  snapshot.VersionNumber,
            //  a => a.ConfirmVersion(FactoryInstanceId, update.BaseVersionNumber),
            //  out current);
            //Trace.TraceVerbose($"Vendor actor #{FactoryInstanceId} of factory '{FactoryId}' confirmed verion #{snapshot.VersionNumber} for base #{update.BaseVersionNumber}");

            //await _busEntity.PublishMessageAsync(new VersionCumulativeUpdateNotification<T>(this, snapshot.VersionNumber, update));

            //return snapshot.VersionNumber;
        }
    }
}
