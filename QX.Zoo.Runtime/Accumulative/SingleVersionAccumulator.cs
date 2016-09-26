using System;
using System.Threading;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  class SingleVersionAccumulator : BaseVersionAccumulator
  {
    private long _confirmationCount;

    public override bool IsConfirmed => _confirmationCount >= DefaultReliableConfirmationsCount;

    public SingleVersionAccumulator(long versionNumber, long baseVersionNumber) : this(versionNumber, baseVersionNumber, new TaskCompletionSource<long>())
    { }

    public SingleVersionAccumulator(long versionNumber, long baseVersionNumber, TaskCompletionSource<long> versionCompletionSource) : base(versionNumber, baseVersionNumber, versionCompletionSource)
    { }

    public override BaseVersionAccumulator ConfirmVersion()
    {
      return Interlocked.Increment(ref _confirmationCount) >= DefaultReliableConfirmationsCount
        ? (BaseVersionAccumulator) new ConfirmedVersionAccumulator(VersionNumber, BaseVersionNumber, VersionCompletionSource)
        : this;
    }

    public override BaseVersionAccumulator RejectVersion()
    {
      return new RejectedVersionAccumulator(VersionNumber, BaseVersionNumber, VersionCompletionSource);
    }

    public override BaseVersionAccumulator MoveVersion(long newVersionNumber)
    {
      return new MovedVersionAccumulator(VersionNumber, BaseVersionNumber, newVersionNumber, VersionCompletionSource);
    }

    protected override VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      newVersionAccumulatorFunc = null;
      return this;
    }

    public override string ToString()
    {
      return string.Format("SingleVersionAccumulator (version={0}, base={1}, confirms={2})", VersionNumber, BaseVersionNumber, _confirmationCount);
    }
  }
}
