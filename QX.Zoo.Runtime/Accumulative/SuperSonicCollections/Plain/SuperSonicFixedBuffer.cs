using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Plain
{
  sealed class SuperSonicFixedBuffer<T>
  {
    private readonly T[] _items;
    private int _count;

    public SuperSonicFixedBuffer(int bufferSize = 16)
    {
      _items = new T[bufferSize];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSize()
    {
      return _items.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
      var position = Interlocked.Increment(ref _count);
      if (position >= _items.Length) return false;

      _items[position - 1] = item;

      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Flush(out T[] items)
    {
      items = _items;

      return _count;
    }

    public void Reset()
    {
      if (_count > 0)
      {
        Array.Clear(_items, 0, _count);
        _count = 0;
      }
    }
  }
}