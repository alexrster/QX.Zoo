using QX.Zoo.Accumulative;
using QX.Zoo.Hold;

namespace QX.Zoo.Primitive.Accumulative
{
  class AccumulatedVersion : ICloneable<AccumulatedVersion>, 
    ICumulativeUpdateAccumulator<AccumulatedVersion, ConfirmAccumulatedVersion>, 
    ICumulativeUpdateAccumulator<AccumulatedVersion, MoveAndConfirmAccumulatedVersion>
  {
    public long FactoryInstanceId { get; set; }
    public long BaseVersionNumber { get; set; }
    public long VersionNumber { get; set; }

    public void CopyFrom(AccumulatedVersion source)
    {
      FactoryInstanceId = source.FactoryInstanceId;
      BaseVersionNumber = source.BaseVersionNumber;
      VersionNumber = source.VersionNumber;
    }

    void ICumulativeUpdateAccumulator<AccumulatedVersion, ConfirmAccumulatedVersion>.ApplyUpdate(ConfirmAccumulatedVersion update)
    {
      VersionNumber = update.VersionNumber;
    }

    void ICumulativeUpdateAccumulator<AccumulatedVersion, MoveAndConfirmAccumulatedVersion>.ApplyUpdate(MoveAndConfirmAccumulatedVersion update)
    {
      VersionNumber = update.NewVersionNumber;
    }
  }
}
