namespace QX.Zoo.Hold
{
  public static class HoldExtensions
  {
    public static T Clone<T>(this T obj) where T : ICloneable<T>, new()
    {
      var result = new T();
      result.CopyFrom(obj);

      return result;
    }
  }
}