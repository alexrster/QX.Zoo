using System;

namespace QX.Zoo.Hold
{
  /// <summary>
  /// Represents collection of <typeparamref name="T"/> objects organized as binary tree
  /// </summary>
  /// <typeparam name="T">Collection item type</typeparam>
  public interface ISuperSonicCollection<T> where T : class
  {
    /// <summary>
    /// Update item at position <paramref name="index"/> or create a new one if it doesn't exist
    /// </summary>
    /// <param name="index">Item index</param>
    /// <param name="createFunc">Item create function</param>
    /// <param name="updateFunc">Item update function</param>
    /// <param name="old">Previous item value</param>
    /// <returns>Updated item at <paramref name="index"/></returns>
    T AddOrUpdate(long index, Func<long, T> createFunc, Func<T, T> updateFunc, out T old);
  }
}
