using System;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  class SuperBinaryLeaf<T> : SuperBinaryNode<T> where T : class
  {
    private readonly SuperSonicDataBlock<T> _dataBlock;

    public override long LowIndex => _dataBlock.LowIndex;
    public override long HighIndex => _dataBlock.LowIndex + _dataBlock.Size;

    public SuperBinaryLeaf(long index) : this(SuperSonicDataBlockPool.GetOrCreateDataBlock<T>(index))
    { }

    public SuperBinaryLeaf(SuperSonicDataBlock<T> dataBlock)
    {
      _dataBlock = dataBlock;
    }

    public override T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T old)
    {
      return _dataBlock.AddOrUpdate(index, createFunc, updateFunc, out old);
    }

    public override string ToString()
    {
      return $"{base.ToString()} (based on {_dataBlock})";
    }
  }
}
