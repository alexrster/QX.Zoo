using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QX.Zoo.Accumulative;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;
using QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree;
using QX.Zoo.Runtime.Accumulative.Versions;

namespace QX.Zoo.Runtime.Accumulative.Actors
{
  public class WarehouseActor<T> : IWarehouse<T> where T : ICloneable<T>, new()
  {
    private static readonly Task<T> DefaultSnapshotLoadTask = Task.FromResult(new T());
    private static readonly Snapshot<T> DefaultSnapshot = new ReliableSnapshot<T>(0, DefaultSnapshotLoadTask);

    private readonly Func<long, Task<T>> _externalLoaderFunc;
    private readonly ISuperSonicCollection<Snapshot<T>> _snapshots;

    public WarehouseActor(Func<long, Task<T>> externalLoaderFunc, ILogger log)
    {
      _externalLoaderFunc = externalLoaderFunc;
      _snapshots = new SuperBinaryRoot<Snapshot<T>>(log);
    }

    public virtual Snapshot<T> UpdateSnapshot(ICumulativeUpdate<T> update, long newVersionNumber, out Snapshot<T> current)
    {
      return _snapshots.AddOrUpdate(update.BaseVersionNumber, LoadSnapshot, s => s.ApplyUpdate(newVersionNumber, update), out current);
    }

    //public virtual Snapshot<T> MoveSnapshot(long versionNumber, long newVersionNumber, out Snapshot<T> old)
    //{
    //  if (versionNumber <= 0)
    //  {
    //    throw new ArgumentOutOfRangeException(nameof(versionNumber), versionNumber, "Version number should be greater 0");
    //  }

    //  if (newVersionNumber < versionNumber || versionNumber == 0)
    //  {
    //    throw new ArgumentOutOfRangeException(nameof(newVersionNumber), newVersionNumber, "New version number should be greater than current version number");
    //  }

    //  _snapshots.AddOrUpdate(versionNumber, LoadSnapshot, s => new OutdatedSnapshot<T>(s), out old);

    //  var current = old;
    //  return _snapshots.AddOrUpdate(newVersionNumber, s => current, s => s  .ApplyVersionUpdate(newVersionNumber), out old);
    //}

    //private Snapshot<T> GetOrUpdateSnapshot(Snapshot<T> snapshot, ICumulativeUpdate<T> update, long newIndex)
    //{
    //  return GetOrUpdateSnapshot(snapshot, (i, s) => s.ApplyUpdate(i, update), update.BaseVersionNumber, newIndex);
    //}

    //private Snapshot<T> GetOrUpdateSnapshot(Snapshot<T> snapshot, Func<long, Snapshot<T>, Snapshot<T>> updateFunc, long index, long newIndex)
    //{
    //  return updateFunc(newIndex, snapshot ?? LoadSnapshot(index));
    //}

    private Snapshot<T> LoadSnapshot(long index)
    {
      return index > 0 ? new ReliableSnapshot<T>(index, _externalLoaderFunc(index)) : DefaultSnapshot;
    }
  }
} 
