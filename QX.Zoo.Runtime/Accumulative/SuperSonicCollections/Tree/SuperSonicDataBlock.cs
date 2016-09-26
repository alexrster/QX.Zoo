using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  class SuperSonicDataBlock<T> where T : class
  {
    private readonly long _lowIndex;
    private readonly string _name;
    private readonly T[] _items;

    public long LowIndex => _lowIndex;
    public int Size { get; }

    public SuperSonicDataBlock(long lowIndex, int size, string name)
    {
      _items = new T[size];

      _lowIndex = lowIndex;
      _name = name;
      Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T current)
    {
      current = Volatile.Read(ref _items[index - _lowIndex]);
      var item = current == null ? createFunc(index) : updateFunc(current);

      if (Interlocked.CompareExchange(ref _items[index - _lowIndex], item, current) != current)
      {
        throw new InvalidOperationException($"Item '{index - _lowIndex}' of '{this}' has been changed");
      }

      return item;
    }

    public override string ToString()
    {
      return string.Format(_name, _lowIndex, _lowIndex + Size);
    }
  }
}
