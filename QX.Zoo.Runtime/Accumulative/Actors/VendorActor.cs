using System;
using System.Runtime.CompilerServices;
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
  sealed class VendorActor<T> : IAccumulatingFactory where T : ICloneable<T>, new()
  {
    private readonly LibrarianActor<T> _librarian;
    private readonly Func<IAsyncBusBroker, IAsyncBusEntity> _busEntityFunc;
    private readonly ILogger _log;
    private readonly ISuperSonicCollection<VersionAccumulator> _versions;
    private IAsyncBusEntity _busEntity;
    private int _isInitialized;
    private long _versionNumber;
    private long _confirmedVersionNumber;

    public Guid FactoryId { get; }
    public long FactoryInstanceId { get; }

    internal VendorActor(Guid factoryId, long factoryInstanceId, LibrarianActor<T> librarian, Func<IAsyncBusBroker, IAsyncBusEntity> busEntityFunc, ILogger log)
    {
      _versionNumber = librarian.VersionNumber;
      _confirmedVersionNumber = librarian.VersionNumber + 1;
      _librarian = librarian;
      _busEntityFunc = busEntityFunc;
      _log = log;
      _versions = new SuperBinaryRoot<VersionAccumulator>(log);

      FactoryId = factoryId;
      FactoryInstanceId = factoryInstanceId;
    }

    public async Task StartAsync(IAsyncBusBroker busBroker)
    {
      _log.LogInformation((int)Events.Start, $"Start Vendor actor instance #{FactoryInstanceId} of factory '{FactoryId}'");
      if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
      {
        _log.LogWarning($"Vendor actor #{FactoryInstanceId} of factory '{FactoryId}' already started");
        return;
      }

      _log.LogVerbose($"Start internal Vendor actor instance #{_librarian.FactoryInstanceId} of factory '{FactoryId}'");
      await _librarian.StartAsync(busBroker);

      _log.LogVerbose($"Get async bus entity for Vendor actor #{FactoryInstanceId} of factory '{FactoryId}'");
      _busEntity = _busEntityFunc(busBroker);

      _log.LogVerbose($"Subscribe for notifications from async bus entity '{_busEntity.EntityId}', Vendor actor #{FactoryInstanceId} of factory '{FactoryId}'");
      await Task.WhenAll(
        _busEntity.SubscribeAsync<VersionConfirmNotification>(HandleMessageAsync),
        _busEntity.SubscribeAsync<VersionMoveAndConfirmNotification>(HandleMessageAsync));

      _log.LogInformation($"Vendor actor instance #{FactoryInstanceId} of factory '{FactoryId}' has been successfully started");
    }

    public async Task<Tuple<long, Task<long>>> PromoteNewVersion(IAsyncBusEntity bus, long baseVersionNumber)
    {
      var versionAccumulator = _versions.AddOrUpdate(Interlocked.Increment(ref _versionNumber), v => new SingleVersionAccumulator(v, baseVersionNumber), a => a.ConfirmVersion(baseVersionNumber));
      _log.LogVerbose($"Vendor actor instance #{FactoryInstanceId} of factory '{FactoryId}' promote new version #{versionAccumulator.VersionNumber} for base #{baseVersionNumber}");

      await bus.PublishMessageAsync(new VersionConfirmNotification(FactoryId, FactoryInstanceId, versionAccumulator.VersionNumber, baseVersionNumber));

      return new Tuple<long, Task<long>>(versionAccumulator.VersionNumber, versionAccumulator.ConfirmTask);
    }

    private async Task HandleMessageAsync(IAsyncBusEntity bus, VersionConfirmNotification notification)
    {
      _log.LogInformation($"received 'VersionConfirm(v=#{notification.VersionNumber}, b=#{notification.BaseVersionNumber})' notification from Vendor #{notification.FactoryInstanceId}");
      if (notification.FactoryInstanceId == FactoryInstanceId)
      {
        _log.LogInformation($"skip processing of self generated notification");
        return;
      }

      UpdateGlobalLatestVersionNumber(notification.VersionNumber);

      var versionAccumulator = _versions.AddOrUpdate(notification.VersionNumber,
        v => new SingleVersionAccumulator(v, notification.BaseVersionNumber),
        a => a.ConfirmVersion(notification.BaseVersionNumber));
      _log.LogVerbose($"accumulated version #{versionAccumulator.VersionNumber} for base #{notification.BaseVersionNumber}: {versionAccumulator}");

      await ProcessConfirmedVersions(bus);
      _log.LogInformation($"finished processing 'VersionConfirm(v=#{notification.VersionNumber}, b=#{notification.BaseVersionNumber})' notification from Vendor #{notification.FactoryInstanceId}");
    }

    private async Task HandleMessageAsync(IAsyncBusEntity bus, VersionMoveAndConfirmNotification notification)
    {
      _log.LogInformation($"received 'VersionMoveAndConfirm(new=#{notification.VersionNumber}, old=#{notification.OldVersion}, b=#{notification.BaseVersionNumber})' notification from Vendor #{notification.FactoryInstanceId}");
      if (notification.FactoryInstanceId == FactoryInstanceId)
      {
        _log.LogInformation($"skip processing of self generated notification");
        return;
      }

      UpdateGlobalLatestVersionNumber(notification.VersionNumber);

      var rejectedVersionAccumulator = _versions.AddOrUpdate(notification.OldVersion,
        v => new MovedVersionAccumulator(v, notification.BaseVersionNumber, notification.VersionNumber), 
        a => a.MoveVersion(notification.BaseVersionNumber, notification.VersionNumber));
      _log.LogVerbose($"rejected version #{rejectedVersionAccumulator.VersionNumber} for base #{notification.BaseVersionNumber}");

      var versionAccumulator = _versions.AddOrUpdate(notification.VersionNumber,
        v => new SingleVersionAccumulator(v, notification.BaseVersionNumber),
        a => a.ConfirmVersion(notification.BaseVersionNumber));
      _log.LogVerbose($"accumulated version #{versionAccumulator.VersionNumber} for base #{notification.BaseVersionNumber}");

      await ProcessConfirmedVersions(bus);
      _log.LogInformation($"finished processing 'VersionMoveAndConfirm(new=#{notification.VersionNumber}, old=#{notification.OldVersion}, b=#{notification.BaseVersionNumber})' notification from Vendor #{notification.FactoryInstanceId}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateGlobalLatestVersionNumber(long versionNumber)
    {
      var latestVersionNumber = Volatile.Read(ref _versionNumber);
      while (versionNumber > latestVersionNumber && Interlocked.CompareExchange(ref _versionNumber, versionNumber, latestVersionNumber) != latestVersionNumber)
      {
        latestVersionNumber = Volatile.Read(ref _versionNumber);
      }
    }

    private async Task ProcessConfirmedVersions(IAsyncBusEntity bus)
    {
      Func<long, VersionAccumulator> moveVersionFunc = null;
      var confirmedVersionNumber = Volatile.Read(ref _confirmedVersionNumber);
      var versionNumber = Volatile.Read(ref _versionNumber);

      _log.LogVerbose($"Start process confirmed versions from #{confirmedVersionNumber} to #{versionNumber}");

      while (confirmedVersionNumber <= versionNumber)
      {
        _log.LogInformation($"Processing version #{confirmedVersionNumber}");

        VersionAccumulator originalAccumulator;
        var versionAccumulator = _versions.AddOrUpdate(confirmedVersionNumber, v => null, a => a.ApproveOrMoveVersion(out moveVersionFunc), out originalAccumulator);
        if (versionAccumulator == null)
        {
          _log.LogInformation($"Version #{confirmedVersionNumber} doesn't exist");
          return;
        }

        if (versionAccumulator.IsCompleted && originalAccumulator?.IsCompleted == true)
        {
          _log.LogVerbose($"Version #{confirmedVersionNumber} has been already approved or moved");
          confirmedVersionNumber = Interlocked.Increment(ref _confirmedVersionNumber);

          continue;
        }

        if (versionAccumulator.IsCompleted || (!versionAccumulator.IsCompleted && confirmedVersionNumber == versionNumber))
        {
          var baseVersion = (versionAccumulator as BaseVersionAccumulator)?.BaseVersionNumber;
          if (baseVersion == null)
          {
            throw new InvalidOperationException($"Version accumulator #{versionAccumulator.VersionNumber} cannot be completed with multiple base versions");
          }

          if (moveVersionFunc != null)
          {
            _log.LogInformation((int)Events.Moving, $"Moving version #{versionAccumulator.VersionNumber} for base #{baseVersion}");
            VersionAccumulator currentAccumulator;
            do
            {
              versionNumber = Interlocked.Increment(ref _versionNumber);
              _versions.AddOrUpdate(versionNumber, moveVersionFunc, a => a, out currentAccumulator);
            } while (currentAccumulator != null);

            _log.LogInformation((int)Events.Moved, $"Moved version #{versionAccumulator.VersionNumber} for base #{baseVersion} to version #{versionNumber}");
            await bus.PublishMessageAsync(new VersionMoveAndConfirmNotification(FactoryId, FactoryInstanceId, versionNumber, baseVersion.Value, versionAccumulator.VersionNumber));
          }
          else
          {
            _log.LogInformation((int)Events.Approved, $"Approved version #{versionAccumulator.VersionNumber} for base #{baseVersion}");
            await bus.PublishMessageAsync(new VersionConfirmNotification(FactoryId, FactoryInstanceId, versionAccumulator.VersionNumber, baseVersion.Value));
          }

          confirmedVersionNumber = Interlocked.Increment(ref _confirmedVersionNumber);
          if (confirmedVersionNumber > versionNumber)
          {
            if (Interlocked.CompareExchange(ref _versionNumber, confirmedVersionNumber, versionNumber) == versionNumber)
            {
              _log.LogInformation((int)Events.SetVersion, $"Set last version number #{confirmedVersionNumber}");
            }

            return;
          }
        }
      }
    }

    private enum Events
    {
      Start = 0x10001,
      Moving,
      Moved,
      Approved,
      SetVersion
    }
  }
}
