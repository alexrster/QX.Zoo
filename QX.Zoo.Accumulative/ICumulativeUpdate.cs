using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative
{
  /// <summary>
  /// Atomic cumulative update information interface
  ///
  /// Cumulative update is based on specific version <see cref="BaseVersionNumber"/> of cumulative object <typeparamref name="T" />
  /// </summary>
  public interface ICumulativeUpdate
  {
    /// <summary>
    /// Accumulating object version on which cumulative update is based on
    /// </summary>
    long BaseVersionNumber { get; }
  }

  /// <summary>
  /// Atomic cumulative update information interface
  ///
  /// Cumulative update is based on specific version cumulative object <typeparamref name="T" />
  /// </summary>
  public interface ICumulativeUpdate<in T> : ICumulativeUpdate where T : ICloneable<T>, new()
  {
    /// <summary>
    /// Apply update on target <paramref name="obj" />
    /// </summary>
    /// <param name="obj">Target cumulative object</param>
    void Apply(T obj);
  }

  /// <summary>
  /// Atomic cumulative update information interface which define basic rules for cumulative update handlers implementation
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="TUpdate"></typeparam>
  public interface ICumulativeUpdate<in T, in TUpdate> : ICumulativeUpdate<T>
    where T : ICloneable<T>, ICumulativeUpdateAccumulator<T, TUpdate>, new()
    where TUpdate : ICumulativeUpdate<T, TUpdate>
  { }
}
