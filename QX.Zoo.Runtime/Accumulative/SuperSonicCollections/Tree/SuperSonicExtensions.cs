using System;
using System.Runtime.CompilerServices;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  static class SuperSonicExtensions
  {
    public static long GetMeridian<T>(this SuperBinaryNode<T> leftNode, SuperBinaryNode<T> rightNode) where T : class 
      => SuperSonicDataBlockPool<T>.DataBlockSize * ((leftNode.LowIndex + rightNode.HighIndex) / (2 * SuperSonicDataBlockPool<T>.DataBlockSize) + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AddOrUpdate<T>(this ISuperSonicCollection<T> collection, long index, Func<long, T> createFunc, Func<T, T> updateFunc) where T : class
    {
      T old;
      return collection.AddOrUpdate(index, createFunc, updateFunc, out old);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(this ISuperSonicCollection<T> collection, long index, T defaultValue = default(T)) where T : class
    {
      return collection.AddOrUpdate(index, x => defaultValue, x => x);
    }
  }
}
