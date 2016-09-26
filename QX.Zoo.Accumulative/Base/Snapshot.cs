using System;
using System.Threading.Tasks;
using QX.Zoo.Accumulative.Exceptions;
using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative.Base
{
  public abstract class Snapshot : IAccumulatedVersion
  {
    public long VersionNumber { get; }

    protected Snapshot(long versionNumber)
    {
      VersionNumber = versionNumber;
    }

    protected abstract Snapshot ApplyUpdateInternal(long versionNumber, ICumulativeUpdate update);
    protected abstract Snapshot ApplyVersionUpdateInternal(long newVersionNumber);
  }

  public abstract class Snapshot<T> : Snapshot where T : ICloneable<T>, new()
  {
    private readonly Lazy<Task<T>> _lazyStateLoader;

    protected Snapshot(long versionNumber) : base(versionNumber)
    {
      _lazyStateLoader = new Lazy<Task<T>>(LoadStateAsync);
    }

    public Snapshot<T> ApplyUpdate(long versionNumber, ICumulativeUpdate<T> update)
    {
      if (VersionNumber != update.BaseVersionNumber)
      {
        throw new InvalidVersionException($"Base version mismatch - update based on #{update.BaseVersionNumber} but current version is #{VersionNumber}");
      }

      return ApplyUpdateInternal(versionNumber, update);
    }

    public Snapshot<T> ApplyVersionUpdate(long newVersionNumber)
    {
      return (Snapshot<T>)ApplyVersionUpdateInternal(newVersionNumber);
    }

    public async Task<T> GetStateAsync()
    {
      return (await _lazyStateLoader.Value).Clone();
    }

    protected abstract Task<T> LoadStateAsync();
    protected abstract Snapshot<T> ApplyUpdateInternal(long versionNumber, ICumulativeUpdate<T> update);

    protected sealed override Snapshot ApplyUpdateInternal(long versionNumber, ICumulativeUpdate update)
    {
      return ApplyUpdateInternal(versionNumber, (ICumulativeUpdate<T>)update);
    }
  }
}
