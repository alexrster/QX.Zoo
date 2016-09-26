using System;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  class MovedVersionAccumulator : BaseVersionAccumulator
  {
    public long NewVersionNumber { get; }

    public MovedVersionAccumulator(long versionNumber, long baseVersionNumber, long newVersionNumber) 
      : this(versionNumber, baseVersionNumber, newVersionNumber, new TaskCompletionSource<long>())
    { }

    public MovedVersionAccumulator(long versionNumber, long baseVersionNumber, long newVersionNumber, TaskCompletionSource<long> versionCompletionSource) 
      : base(versionNumber, baseVersionNumber, versionCompletionSource)
    {
      NewVersionNumber = newVersionNumber;
    }

    public override BaseVersionAccumulator ConfirmVersion()
    {
      return this;
    }

    public override BaseVersionAccumulator RejectVersion()
    {
      return new RejectedVersionAccumulator(VersionNumber, BaseVersionNumber, VersionCompletionSource);
    }

    public override BaseVersionAccumulator MoveVersion(long newVersionNumber)
    {
      if (newVersionNumber > VersionNumber)
      {
        return new MovedVersionAccumulator(VersionNumber, BaseVersionNumber, newVersionNumber, VersionCompletionSource);
      }

      return this;
    }

    protected override VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      newVersionAccumulatorFunc = null;
      return this;
    }
  }
}
