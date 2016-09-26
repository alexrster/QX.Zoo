using System.Threading.Tasks;
using QX.Zoo.Accumulative;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Accumulative.Exceptions;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.Versions
{
  public class OutdatedVirtualSnapshot<T> : Snapshot<T> where T : ICloneable<T>, new()
  {
    public OutdatedVirtualSnapshot(long versionNumber) : base(versionNumber)
    { }

    protected override Task<T> LoadStateAsync()
    {
      throw new InvalidVersionException($"Can't load already outdated version #{VersionNumber}");
    }

    protected override Snapshot<T> ApplyUpdateInternal(long newVersionNumber, ICumulativeUpdate<T> update)
    {
      throw new InvalidVersionException($"Can't apply update #{newVersionNumber} on object instance #{VersionNumber} - object outdated");
    }

    protected override Snapshot ApplyVersionUpdateInternal(long newVersionNumber)
    {
      throw new InvalidVersionException($"Can't apply version update #{newVersionNumber} on object instance #{VersionNumber} - object outdated");
    }
  }
}
