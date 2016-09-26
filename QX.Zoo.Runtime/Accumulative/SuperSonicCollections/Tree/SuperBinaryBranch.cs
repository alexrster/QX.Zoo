using System;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Logging;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  class SuperBinaryBranch<T> : SuperBinaryRoot<T> where T : class
  {
    private readonly ILogger _log;

    private SuperBinaryNode<T> _leftNode;
    private SuperBinaryNode<T> _rightNode;

    public override long LowIndex => _leftNode.LowIndex;
    public override long HighIndex => _rightNode.HighIndex;

    public SuperBinaryBranch(long index, SuperBinaryNode<T> rightNode, ILogger log) : this(new SuperBinaryLeaf<T>(index), rightNode, log)
    { }

    public SuperBinaryBranch(SuperBinaryNode<T> leftNode, long index, ILogger log) : this(leftNode, new SuperBinaryLeaf<T>(index), log)
    { }

    public SuperBinaryBranch(SuperBinaryNode<T> leftNode, SuperBinaryNode<T> rightNode, ILogger log) : base(log)
    {
      _leftNode = leftNode;
      _rightNode = rightNode;
      _log = log;
    }

    public override T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T old)
    {
      _log.LogVerbose("Start add or update item at index {0}", index);
      var result = GetOrCreateNode(index).AddOrUpdate(index, createFunc, updateFunc, out old);

      _log.LogVerbose("End add or update item at index {0}: old={1}, new={2}", index, old, result);
      return result;
    }

    private SuperBinaryNode<T> GetOrCreateNode(long index)
    {
      _log.LogVerbose("Start GetOrCreate SuperBinary Branch for index {0}", index);

      SuperBinaryNode<T> old;
      var node = index < this.GetMeridian(_rightNode) 
        ? GetOrCreateAndSwitchNode(index, (i, n) => i >= n.LowIndex, i => new SuperBinaryBranch<T>(i, this, _log), ref _leftNode, out old)
        : GetOrCreateAndSwitchNode(index, (i, n) => i < n.HighIndex, i => new SuperBinaryBranch<T>(this, i, _log), ref _rightNode, out old);

      _log.LogVerbose("Finished GetOrCreate SuperBinary Branch for index '{0}': new='{2}', old='{1}'", index, old, node);
      return node;
    }
  }
}
