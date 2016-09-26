using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative
{
  /// <summary>
  /// Accumulator for cumulative update <typeparamref name="TUpdate"/>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="TUpdate"></typeparam>
  public interface ICumulativeUpdateAccumulator<T, in TUpdate>
    where T : ICloneable<T>, new()
    where TUpdate : ICumulativeUpdate<T>
  {
    /// <summary>
    /// Accumulate cumulative update <typeparamref name="TUpdate"/>
    /// </summary>
    /// <param name="update">Cumulative update</param>
    void ApplyUpdate(TUpdate update);
  }
}