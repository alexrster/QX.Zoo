using QX.Zoo.Accumulative.Base;

namespace QX.Zoo.Primitive.Accumulative
{
  abstract class ConfirmAccumulatedVersion : CumulativeUpdate<AccumulatedVersion, ConfirmAccumulatedVersion>
  {
    public long FactoryInstanceId { get; set; }
    public long VersionNumber { get; set; }
  }
}