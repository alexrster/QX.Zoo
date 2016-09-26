using System;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  abstract class SuperBinaryNode<T> : ISuperSonicCollection<T> where T : class
  {
    public abstract long LowIndex { get; }
    public abstract long HighIndex { get; }

    public abstract T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T old);

    public override string ToString()
    {
      return $"SuperBinaryNode {LowIndex}-{HighIndex}";
    }
  }
}
