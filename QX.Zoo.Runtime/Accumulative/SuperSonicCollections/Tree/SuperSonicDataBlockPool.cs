using System.Threading;

namespace QX.Zoo.Runtime.Accumulative.SuperSonicCollections.Tree
{
  static class SuperSonicDataBlockPool
  {
    private static long _counter;

    public static SuperSonicDataBlock<T> GetOrCreateDataBlock<T>(long startIndex) where T : class
    {
      return SuperSonicDataBlockPool<T>.GetOrCreateDataBlock(startIndex, $"SuperSonic data block #{Interlocked.Increment(ref _counter)}, range: {{0}}-{{1}}");
    }
  }

  static class SuperSonicDataBlockPool<T> where T : class
  {
    public static int DataBlockSize = 4;

    public static SuperSonicDataBlock<T> GetOrCreateDataBlock(long startIndex, string name)
    {
      return new SuperSonicDataBlock<T>(DataBlockSize * (startIndex / DataBlockSize), DataBlockSize, name);
    }
  }
}
