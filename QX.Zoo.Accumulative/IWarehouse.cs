using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative
{
  public interface IWarehouse<T> where T : ICloneable<T>, new()
  {
    Snapshot<T> UpdateSnapshot(ICumulativeUpdate<T> update, long newVersionNumber, out Snapshot<T> current);
  }
}
