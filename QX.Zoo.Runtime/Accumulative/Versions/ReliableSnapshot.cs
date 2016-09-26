using System.Threading.Tasks;
using QX.Zoo.Accumulative;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Accumulative.Exceptions;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.Versions
{
  public class ReliableSnapshot<T> : Snapshot<T> where T : ICloneable<T>, new()
  {
    private readonly Task<T> _stateLoaderTask;

    public ReliableSnapshot(long versionNumber, Task<T> stateLoaderTask) : base(versionNumber)
    {
      _stateLoaderTask = stateLoaderTask;
    }

    protected override Snapshot<T> ApplyUpdateInternal(long newVersionNumber, ICumulativeUpdate<T> update)
    {
      if (newVersionNumber <= VersionNumber)
      {
        throw new InvalidVersionException($"Can't apply update #{newVersionNumber} with base #{update.BaseVersionNumber} - current object version is already #{VersionNumber}");
      }

      return new UnconfirmedSnapshot<T>(newVersionNumber, this, update);
    }

    protected override Snapshot ApplyVersionUpdateInternal(long newVersionNumber)
    {
      throw new InvalidVersionException($"Can't apply version update #{newVersionNumber} for object #{VersionNumber} - incorrect state");
    }

    protected sealed override Task<T> LoadStateAsync()
    {
      return _stateLoaderTask;
    }
  }
}
