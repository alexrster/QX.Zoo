using System;
using System.Threading;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  enum ConfirmationStatus
  {
    Unconfirmed,
    Confirmed,
    Moved,
    Rejected
  }

  abstract class VersionAccumulator
  {
    protected const int DefaultReliableConfirmationsCount = 2;

    public long VersionNumber { get; private set; }

    public virtual bool IsCompleted => false;
    public virtual bool IsConfirmed => false;

    public abstract Task<long> ConfirmTask { get; }

    protected VersionAccumulator(long versionNumber)
    {
      VersionNumber = versionNumber;
    }

    public abstract VersionAccumulator ConfirmVersion(long baseVersionNumber);
    public abstract VersionAccumulator RejectVersion(long baseVersionNumber);
    public abstract VersionAccumulator MoveVersion(long baseVersionNumber, long newVersionNumber);

    protected abstract VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc);

    public VersionAccumulator ApproveOrMoveVersion(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      if (!IsCompleted)
      {
        newVersionAccumulatorFunc = null;
        return this;
      }

      return ApproveOrMoveVersionInternal(out newVersionAccumulatorFunc);
    }
  }

  class ConfirmationCollection
  {
    public const int DefaultReliableConfirmationsCount = 2;

    private readonly long[] _confirmedInstances;
    private ConfirmationStatus _status = ConfirmationStatus.Unconfirmed;
    private int _index = -1;

    public long NewVersionNumber { get; private set; }

    public ConfirmationStatus ConfimrationStatus => (Volatile.Read(ref _index) + 1 >= _confirmedInstances.Length) && _status == ConfirmationStatus.Unconfirmed ? ConfirmationStatus.Confirmed : _status;

    public ConfirmationCollection(int reliableConfirmationsCount = DefaultReliableConfirmationsCount)
    {
      _confirmedInstances = new long[reliableConfirmationsCount];
    }

    public ConfirmationCollection Confirm(long factoryInstanceId)
    {
      var index = Volatile.Read(ref _index) + 1;
      if (index >= _confirmedInstances.Length)
      {
        return this;
      }

      for (var i = 0; i < index; i++)
      {
        if (_confirmedInstances[i] == factoryInstanceId)
        {
          return this;
        }
      }

      index = Interlocked.Increment(ref _index);
      if (index < _confirmedInstances.Length)
      {
        _confirmedInstances[index] = factoryInstanceId;
      }

      return this;
    }

    public ConfirmationCollection Reject()
    {
      if (_status == ConfirmationStatus.Unconfirmed)
      {
        _status = ConfirmationStatus.Rejected;
      }

      return this;
    }

    public ConfirmationCollection Move(long newVersionNumber)
    {
      if (_status == ConfirmationStatus.Unconfirmed)
      {
        _status = ConfirmationStatus.Moved;
        NewVersionNumber = newVersionNumber;
      }

      return this;
    }
  }
}
