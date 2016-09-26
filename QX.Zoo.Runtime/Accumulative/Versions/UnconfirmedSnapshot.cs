using System.Threading.Tasks;
using QX.Zoo.Accumulative;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.Versions
{
  public class UnconfirmedSnapshot<T> : Snapshot<T> where T : ICloneable<T>, new()
  {
    private readonly ICumulativeUpdate<T> _update;
    private readonly Snapshot<T> _snapshot;

    public UnconfirmedSnapshot(long versionNumber, Snapshot<T> snapshot, ICumulativeUpdate<T> update) : base(versionNumber)
    {
      _update = update;
      _snapshot = snapshot;
    }

    public long GetBaseVersion()
    {
      return _snapshot.VersionNumber;
    }

    protected override Snapshot<T> ApplyUpdateInternal(long newVersionNumber, ICumulativeUpdate<T> update)
    {
      if (newVersionNumber == VersionNumber)
      {
        return new ReliableSnapshot<T>(newVersionNumber, GetStateAsync());
      }

      return new UnconfirmedSnapshot<T>(newVersionNumber, _snapshot.ApplyUpdate(newVersionNumber, update), update);
    }

    protected override Snapshot ApplyVersionUpdateInternal(long newVersionNumber)
    {
      if (newVersionNumber == VersionNumber)
      {
        return new ReliableSnapshot<T>(newVersionNumber, GetStateAsync());
      }

      return new UnconfirmedSnapshot<T>(newVersionNumber, _snapshot, _update);
    }

    protected override Task<T> LoadStateAsync()
    {
      return _update.Apply(_snapshot);
    }
  }
}
