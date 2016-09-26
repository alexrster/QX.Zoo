using System.Threading.Tasks;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.Versions
{
  public sealed class OutdatedSnapshot<T> : OutdatedVirtualSnapshot<T> where T : ICloneable<T>, new()
  {
    private readonly Snapshot<T> _snapshot;

    public OutdatedSnapshot(Snapshot<T> snapshot) : base(snapshot.VersionNumber)
    {
      _snapshot = snapshot;
    }

    protected override Task<T> LoadStateAsync()
    {
      return _snapshot.GetStateAsync();
    }
  }
}
