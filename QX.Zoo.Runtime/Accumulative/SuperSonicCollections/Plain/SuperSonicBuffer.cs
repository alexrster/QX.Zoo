using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Plain
{
  sealed class SuperSonicBuffer<T> : IDisposable
  {
    private readonly ConcurrentQueue<SuperSonicFixedBuffer<T>> _fullBuffersQueue = new ConcurrentQueue<SuperSonicFixedBuffer<T>>();
    private readonly ConcurrentQueue<SuperSonicFixedBuffer<T>> _emptyBuffersQueue = new ConcurrentQueue<SuperSonicFixedBuffer<T>>();
    private SuperSonicFixedBuffer<T> _headBuffer = new SuperSonicFixedBuffer<T>();

    public void Add(T item)
    {
      var currentHeadBuffer = Volatile.Read(ref _headBuffer);
      while (!currentHeadBuffer.Add(item))
      {
        SwitchHeadBuffer(ref currentHeadBuffer);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetNextEmptyBuffer(out SuperSonicFixedBuffer<T> buffer)
    {
      if (!_emptyBuffersQueue.TryDequeue(out buffer))
      {
        buffer = new SuperSonicFixedBuffer<T>();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SwitchHeadBuffer(ref SuperSonicFixedBuffer<T> headBuffer)
    {
      var currentBuffer = Volatile.Read(ref headBuffer);
      GetNextEmptyBuffer(out headBuffer);
      if (Interlocked.CompareExchange(ref _headBuffer, headBuffer, currentBuffer) != currentBuffer)
      {
        _emptyBuffersQueue.Enqueue(headBuffer);
        headBuffer = Volatile.Read(ref _headBuffer);
      }
    }

    public void Dispose()
    {
      _headBuffer = null;

      SuperSonicFixedBuffer<T> item;
      while (_fullBuffersQueue.TryDequeue(out item)) { }
      while (_emptyBuffersQueue.TryDequeue(out item)) { }
    }
  }
}
