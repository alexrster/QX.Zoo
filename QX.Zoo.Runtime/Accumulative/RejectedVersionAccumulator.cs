using System;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  sealed class RejectedVersionAccumulator : BaseVersionAccumulator
  {
    public override bool IsCompleted => true;
    public override bool IsConfirmed => false;

    public RejectedVersionAccumulator(long versionNumber, long baseVersionNumber, TaskCompletionSource<long> versionCompletionSource) : base(versionNumber, baseVersionNumber, versionCompletionSource)
    { }

    public override BaseVersionAccumulator ConfirmVersion()
    {
      return this;
    }

    public override BaseVersionAccumulator RejectVersion()
    {
      return this;
    }

    public override BaseVersionAccumulator MoveVersion(long newVersionNumber)
    {
      return this;
    }

    protected override VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      //Trace.TraceVerbose($"Cancel wait handle of REJECTED version #{VersionNumber} for base #{BaseVersionNumber}");
      VersionCompletionSource.TrySetCanceled();

      newVersionAccumulatorFunc = null;
      return this;
    }
  }
}
