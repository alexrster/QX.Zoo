using System;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  sealed class ConfirmedVersionAccumulator : BaseVersionAccumulator
  {
    public override bool IsCompleted => true;
    public override bool IsConfirmed => true;

    public ConfirmedVersionAccumulator(long versionNumber, long baseVersionNumber, TaskCompletionSource<long> versionCompletionSource) : base(versionNumber, baseVersionNumber, versionCompletionSource)
    { }

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
      return new MovedVersionAccumulator(VersionNumber, BaseVersionNumber, newVersionNumber, VersionCompletionSource);
    }

    protected override VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      //Trace.TraceVerbose($"Resolve wait handle of SUCCESSFULLY confirmed version #{VersionNumber} for base #{BaseVersionNumber}");
      VersionCompletionSource.TrySetResult(VersionNumber);

      newVersionAccumulatorFunc = null;
      return this;
    }
  }
}
