using System;
using System.Threading.Tasks;

namespace QX.Zoo.Runtime.Accumulative
{
  class LinkedPairVersionAccumulator : VersionAccumulator
  {
    private readonly BaseVersionAccumulator _leftAccumulator;
    private readonly BaseVersionAccumulator _rightAccumulator;

    public override bool IsCompleted => _leftAccumulator.IsCompleted || _rightAccumulator.IsCompleted;
    public override bool IsConfirmed => _leftAccumulator.IsConfirmed || _rightAccumulator.IsConfirmed;

    public override Task<long> ConfirmTask
    {
      get { throw new InvalidOperationException(); }
    }

    public LinkedPairVersionAccumulator(BaseVersionAccumulator leftAccumulator, BaseVersionAccumulator rightAccumulator) : base(leftAccumulator.VersionNumber)
    {
      _leftAccumulator = leftAccumulator;
      _rightAccumulator = rightAccumulator;
    }
    
    public override VersionAccumulator ConfirmVersion(long baseVersionNumber)
    {
      if (_leftAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator.ConfirmVersion(), _rightAccumulator);
      }

      if (_rightAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator, _rightAccumulator.ConfirmVersion());
      }

      return this;
    }

    public override VersionAccumulator RejectVersion(long baseVersionNumber)
    {
      if (_leftAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator.RejectVersion(), _rightAccumulator);
      }

      if (_rightAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator, _rightAccumulator.RejectVersion());
      }

      return this;
    }

    public override VersionAccumulator MoveVersion(long baseVersionNumber, long newVersionNumber)
    {
      if (newVersionNumber <= VersionNumber)
      {
        return this;
      }

      if (_leftAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator.MoveVersion(newVersionNumber), _rightAccumulator);
      }

      if (_rightAccumulator.BaseVersionNumber == baseVersionNumber)
      {
        return new LinkedPairVersionAccumulator(_leftAccumulator, _rightAccumulator.MoveVersion(newVersionNumber));
      }

      return this;
    }

    protected override VersionAccumulator ApproveOrMoveVersionInternal(out Func<long, VersionAccumulator> newVersionAccumulatorFunc)
    {
      Func<long, VersionAccumulator> versionAccumulatorFunc;
      if (_leftAccumulator.IsCompleted && _leftAccumulator.IsConfirmed)
      {
        newVersionAccumulatorFunc = v => _rightAccumulator.MoveVersion(VersionNumber, v);

        var result = _leftAccumulator.ApproveOrMoveVersion(out versionAccumulatorFunc);
        if (versionAccumulatorFunc != null)
        {
          newVersionAccumulatorFunc += versionAccumulatorFunc;
        }

        return result;
      }

      if (_rightAccumulator.IsCompleted && _rightAccumulator.IsConfirmed)
      {
        newVersionAccumulatorFunc = v => _leftAccumulator.MoveVersion(VersionNumber, v);

        var result = _rightAccumulator.ApproveOrMoveVersion(out versionAccumulatorFunc);
        if (versionAccumulatorFunc != null)
        {
          newVersionAccumulatorFunc += versionAccumulatorFunc;
        }

        return result;
      }

      newVersionAccumulatorFunc = null;
      return this;
    }
  }
}
