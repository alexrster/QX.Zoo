using QX.Zoo.Accumulative.Base;

namespace QX.Zoo.Primitive.Accumulative
{
  class MoveAndConfirmAccumulatedVersion : CumulativeUpdate<AccumulatedVersion, MoveAndConfirmAccumulatedVersion>
  {
    public long FactoryInstanceId { get; set; }
    public long VersionNumber { get; set; }
    public long NewVersionNumber { get; set; }
  }
}