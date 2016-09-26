using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Logging;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  class SuperBinaryRoot<T> : SuperBinaryNode<T> where T : class
  {
    private readonly ILogger _log;

    private SuperBinaryNode<T> _rootNode;

    public override long LowIndex => _rootNode?.LowIndex ?? long.MinValue;
    public override long HighIndex => _rootNode?.HighIndex ?? long.MaxValue;

    public SuperBinaryRoot(ILogger log)
    {
      _log = log;
    }

    public override T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T old)
    {
      SuperBinaryNode<T> oldNode;
      return GetOrCreateAndSwitchNode(index, IsNodeExistsQuickCheck, LeafFactory, ref _rootNode, out oldNode)
        .AddOrUpdate(index, createFunc, updateFunc, out old);
    }

    protected SuperBinaryNode<T> GetOrCreateAndSwitchNode(long index, Func<long, SuperBinaryNode<T>, bool> nodeQuickCheckFunc, Func<long, SuperBinaryNode<T>> nodeFactory, ref SuperBinaryNode<T> targetNodeRef, out SuperBinaryNode<T> old)
    {
      _log.LogVerbose($"Get or create SuperBinaryRoot<{typeof(T).Name}> for index '{index}'");
      SuperBinaryNode<T> node;
      do
      {
        old = Volatile.Read(ref targetNodeRef);
        if (nodeQuickCheckFunc(index, old))
        {
          _log.LogVerbose($"SuperBinaryRoot<{typeof(T).Name}> is OK for index '{index}': '{old}'");
          return old;
        }

        node = nodeFactory(index);
      } while (Interlocked.CompareExchange(ref targetNodeRef, node, old) != old);

      _log.LogVerbose($"SuperBinaryRoot<{typeof(T).Name}> changed: new='{node}', old='{old}'");
      return node;
    }

    private bool IsNodeExistsQuickCheck(long index, SuperBinaryNode<T> node)
    {
      return node?.LowIndex >= index && node.HighIndex < index;
    }

    private SuperBinaryNode<T> LeafFactory(long targetIndex)
    {
      _log.LogVerbose($"Create new SuperBinaryNode<{typeof(T).Name}> for index '{targetIndex}'");

      var node = new SuperBinaryLeaf<T>(targetIndex);
      return new SuperBinaryBranch<T>(node, node, _log);
    }
  }
}
