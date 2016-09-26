namespace QX.Zoo.Hold
{
  /// <summary>
  /// Define contract for copying values from another instance of <typeparamref name="T"/>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ICloneable<in T>
  {
    /// <summary>
    /// Copy values from another instance of <typeparamref name="T"/>
    /// </summary>
    /// <param name="source">Source instance</param>
    void CopyFrom(T source);
  }
}