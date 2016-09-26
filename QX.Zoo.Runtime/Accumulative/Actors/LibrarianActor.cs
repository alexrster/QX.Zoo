using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Accumulative;
using QX.Zoo.Hold;
using QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree;
using QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications;
using QX.Zoo.Runtime.Logging;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Runtime.Accumulative.Actors
{
  /// <summary>
  /// 
  /// Accumulating factory built as set of chained factory nodes
  /// Each node responsible for executing of the very small piece of work
  /// At the moment I have such stuff to be done:
  ///     - external requests handling
  ///     - version stabilization
  ///     - snapshot accumulation
  ///     - synchronization of the data that is not available locally
  ///     - runtime failover protection
  /// 
  /// Node types:
  /// * FrontEndNode - sort of service layer which handle incoming
  ///   requests and behave as accumulating factory external endpoint.
  ///   No validation or calculation is done and shouldn't be done on this 
  ///   layer (and even other layers)
  /// 
  /// * VersionPublisherNode - handle incoming unconfirmed version changes 
  ///   and publish own state updates
  /// 
  /// * SnapshotAccumulatorNode - accumulate snapshot latest state, gives
  ///   super fast search by snapshot version number
  /// 
  /// * LocalTransactionStoreNode - local transaction log storage. Each instance
  ///   of IAccumulatingFactory should keep local transaction log until it has been
  ///   successfully uploaded to the SharedStoreNode
  /// 
  /// * SharedStoreNode - shared storage (DB) that keeps latest snapshot of each
  ///   factory object. The slowest part of the accumulating factory
  /// 
  /// </summary>

  class LibrarianActor<T> : IAccumulatingFactory where T : ICloneable<T>, new()
  {
    private readonly Func<IAsyncBusBroker, IAsyncBusEntity> _busEntityFunc;
    private readonly ILogger _log;
    private readonly ISuperSonicCollection<IDictionary<long, ICumulativeUpdate<T>>> _updates;
    private int _isInitialized;

    public Guid FactoryId { get; }
    public long FactoryInstanceId { get; }

    public long VersionNumber { get; }

    public LibrarianActor(Guid factoryId, long factoryInstanceId, long versionNumber, Func<IAsyncBusBroker, IAsyncBusEntity> busEntityFunc, ILogger log)
    {
      FactoryId = factoryId;
      FactoryInstanceId = factoryInstanceId;
      VersionNumber = versionNumber;
      _busEntityFunc = busEntityFunc;
      _log = log;
      _updates = new SuperBinaryRoot<IDictionary<long, ICumulativeUpdate<T>>>(log);
    }

    public async Task StartAsync(IAsyncBusBroker busBroker)
    {
      _log.LogInformation($"Start Librarian actor instance #{FactoryInstanceId} of factory '{FactoryId}'");
      if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
      {
        _log.LogInformation($"Librarian actor #{FactoryInstanceId} of factory '{FactoryId}' already started");
        return;
      }

      _log.LogInformation($"Get async bus entity for Librarian actor #{FactoryInstanceId} of factory '{FactoryId}'");
      var busEntity = _busEntityFunc(busBroker);

      _log.LogInformation($"Subscribe for notifications from async bus entity '{busEntity.EntityId}', Librarian actor #{FactoryInstanceId} of factory '{FactoryId}'");
      await busEntity.SubscribeAsync<VersionCumulativeUpdateNotification<T>>(HandleMessageAsync);

      _log.LogInformation($"Librarian actor instance #{FactoryInstanceId} of factory '{FactoryId}' has been successfully started");
    }

    public Task HandleMessageAsync(IAsyncBusEntity sender, VersionCumulativeUpdateNotification<T> notification)
    {
      _log.LogInformation($"Librarian actor #{FactoryInstanceId} of factory '{FactoryId}' received notification of type '{notification.GetType()}' from async bus entity '{sender.EntityId}'");
      _log.LogVerbose($"VersionCumulativeUpdate: base=#{notification.CumulativeUpdate.BaseVersionNumber}, version=#{notification.VersionNumber}, cumulativeUpdate='{notification.CumulativeUpdate.GetType()}'");

      _updates.AddOrUpdate(notification.CumulativeUpdate.BaseVersionNumber, 
        v => new Dictionary<long, ICumulativeUpdate<T>>{{ notification.FactoryInstanceId, notification.CumulativeUpdate }},
        d => { d.Add(notification.FactoryInstanceId, notification.CumulativeUpdate); return d; });

      return Task.FromResult(0);
    }
  }
}
