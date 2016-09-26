using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative.Base
{
  public abstract class CumulativeUpdate<T, TUpdate> : ICumulativeUpdate<T, TUpdate>
    where T : ICloneable<T>, ICumulativeUpdateAccumulator<T, TUpdate>, new()
    where TUpdate : CumulativeUpdate<T, TUpdate>, ICumulativeUpdate<T, TUpdate>
  {
    public long BaseVersionNumber { get; set; }

    public void Apply(T obj)
    {
      obj.ApplyUpdate((TUpdate) this);
    }
  }
}
