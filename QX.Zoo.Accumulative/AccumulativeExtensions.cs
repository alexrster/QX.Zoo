using System.Threading.Tasks;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;

namespace QX.Zoo.Accumulative
{
  public static class AccumulativeExtensions
  {
    public static async Task<T> Apply<T>(this ICumulativeUpdate<T> update, Snapshot<T> snapshot) where T : ICloneable<T>, new()
    {
      var state = await snapshot.GetStateAsync();
      update.Apply(state);

      return state;
    }
  }
}