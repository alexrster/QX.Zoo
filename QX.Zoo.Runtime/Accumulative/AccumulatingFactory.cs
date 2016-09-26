using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Accumulative;
using QX.Zoo.Hold;
using QX.Zoo.Runtime.Accumulative.Actors;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Runtime.Accumulative
{
  public sealed class AccumulatingFactory<T> : IAccumulatingFactory<T> where T : ICloneable<T>, new()
  {
    private readonly ILogger _log;
    private readonly VendorActor<T> _vendorActor;
    private readonly LibrarianActor<T> _librarianNode;
    private readonly TrollActor<T> _trollActor;

    public Guid FactoryId { get; }
    public long FactoryInstanceId { get; }

    public AccumulatingFactory(Guid factoryId, long factoryInstanceId, long confirmedNodeVersion,
      Func<IAsyncBusBroker, IAsyncBusEntity> busEntityFunc,
      Func<object, ILogger> loggerFactory)
    {
      _log = loggerFactory($"QX.Zoo.AccumulatingFactory[{factoryId}][{factoryInstanceId}]");

      FactoryId = factoryId;
      FactoryInstanceId = factoryInstanceId;

      _librarianNode = new LibrarianActor<T>(FactoryId, FactoryInstanceId, confirmedNodeVersion, busEntityFunc, loggerFactory($"QX.Zoo.AccumulatingFactory[{factoryId}].[{factoryInstanceId}].LibrarianActor"));
      _vendorActor = new VendorActor<T>(FactoryId, FactoryInstanceId, _librarianNode, busEntityFunc, loggerFactory($"QX.Zoo.AccumulatingFactory[{factoryId}].[{factoryInstanceId}]VendorActor"));
      _trollActor = new TrollActor<T>(FactoryId, FactoryInstanceId, _vendorActor, busEntityFunc, loggerFactory($"QX.Zoo.AccumulatingFactory[{factoryId}].[{factoryInstanceId}].TrollActor"));
    }

    public Task StartAsync(IAsyncBusBroker busBroker)
    {
      _log.LogInformation($"Starting accumulating factory #{FactoryInstanceId}/{FactoryId}");
      return Task.WhenAll(_vendorActor.StartAsync(busBroker), _librarianNode.StartAsync(busBroker), _trollActor.StartAsync(busBroker));
    }

    public async Task<long> ApplyUpdateAsync(ICumulativeUpdate<T> update)
    {
      using (_log.BeginScope($"{update.GetType().Name}:{update.BaseVersionNumber}"))
      {
        _log.LogInformation($"Start apply update for base #{update.BaseVersionNumber} of type {update.GetType().Name}");
        var versionNumberTask = _trollActor.ApplyUpdateAsync(update);
        if (await Task.WhenAny(versionNumberTask, Task.Delay(TimeSpan.FromSeconds(15))) != versionNumberTask)
        {
          _log.LogError($"Version update confirmation for base #{update.BaseVersionNumber} has not been received during 15 seconds.");
          throw new TimeoutException($"Version update confirmation for #{update.BaseVersionNumber} has not been received during 15 seconds.");
        }

        _log.LogInformation($"Finished apply update for base #{update.BaseVersionNumber} - new object version #{versionNumberTask.Result}");
        return versionNumberTask.Result;
      }
    }
  }
}
