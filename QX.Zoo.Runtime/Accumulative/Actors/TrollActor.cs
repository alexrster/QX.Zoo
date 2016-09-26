using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Accumulative;
using QX.Zoo.Hold;
using QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications;
using QX.Zoo.Runtime.Logging;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Runtime.Accumulative.Actors
{
  class TrollActor<T> : IAccumulatingFactory<T> where T : ICloneable<T>, new()
  {
    private readonly Func<IAsyncBusBroker, IAsyncBusEntity> _busConfigFunc;
    private readonly VendorActor<T> _vendor;
    private readonly ILogger _log;
    private IAsyncBusEntity _bus;

    public Guid FactoryId { get; }
    public long FactoryInstanceId { get; }

    public TrollActor(Guid factoryId, long factoryInstanceId, VendorActor<T> vendor, Func<IAsyncBusBroker, IAsyncBusEntity> busConfigFunc, ILogger log)
    {
      FactoryId = factoryId;
      FactoryInstanceId = factoryInstanceId;
      _vendor = vendor;
      _busConfigFunc = busConfigFunc;
      _log = log;
    }

    public Task StartAsync(IAsyncBusBroker busBroker)
    {
      _log.LogInformation($"Start Troll actor '{FactoryInstanceId}' of factory '{FactoryId}'");

      _bus = _busConfigFunc(busBroker);
      return Task.FromResult(0);
    }

    public async Task<long> ApplyUpdateAsync(ICumulativeUpdate<T> cumulativeUpdate)
    {
      _log.LogInformation($"Begin apply update for base #{cumulativeUpdate.BaseVersionNumber} of type {cumulativeUpdate.GetType().Name}");

      var versionTuple = await _vendor.PromoteNewVersion(_bus, cumulativeUpdate.BaseVersionNumber);
      _log.LogVerbose($"Vendor promoted new version #{versionTuple.Item1} for base #{cumulativeUpdate.BaseVersionNumber}");

      await _bus.PublishMessageAsync(new VersionCumulativeUpdateNotification<T>(FactoryId, FactoryInstanceId, versionTuple.Item1, cumulativeUpdate));

      _log.LogVerbose($"Wait for initial verision #{versionTuple.Item1} to be confirmed or moved by other guys");
      return await versionTuple.Item2;
    }
  }
}
