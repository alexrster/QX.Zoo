using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  abstract class BaseVersionAccumulator : VersionAccumulator
  {
    protected readonly TaskCompletionSource<long> VersionCompletionSource;

    public long BaseVersionNumber { get; }

    public override Task<long> ConfirmTask => VersionCompletionSource.Task;

    protected BaseVersionAccumulator(long versionNumber, long baseVersionNumber, TaskCompletionSource<long> versionCompletionSource) : base(versionNumber)
    {
      BaseVersionNumber = baseVersionNumber;
      VersionCompletionSource = versionCompletionSource;
    }

    public abstract BaseVersionAccumulator ConfirmVersion();
    public abstract BaseVersionAccumulator RejectVersion();
    public abstract BaseVersionAccumulator MoveVersion(long newVersionNumber);

    public sealed override VersionAccumulator ConfirmVersion(long baseVersionNumber)
    {
      if (baseVersionNumber != BaseVersionNumber)
      {
        return this;
      }

      return ConfirmVersion();
    }

    public sealed override VersionAccumulator RejectVersion(long baseVersionNumber)
    {
      if (baseVersionNumber != BaseVersionNumber)
      {
        return this;
      }

      return RejectVersion();
    }

    public sealed override VersionAccumulator MoveVersion(long baseVersionNumber, long newVersionNumber)
    {
      if (newVersionNumber <= VersionNumber)
      {
        return this;
      }

      if (baseVersionNumber != BaseVersionNumber)
      {
        return this;
      }

      return MoveVersion(newVersionNumber);
    }
  }
}
