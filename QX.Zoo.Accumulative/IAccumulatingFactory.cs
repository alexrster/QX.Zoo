using System;
using System.Threading.Tasks;
using QX.Zoo.Hold;
using QX.Zoo.Talk.MessageBus;

namespace QX.Zoo.Accumulative
{
  /// <summary>
  /// Accumulating factory
  /// </summary>
  public interface IAccumulatingFactory
  {
    /// <summary>
    /// Unique Id
    /// </summary>
    Guid FactoryId { get; }

    /// <summary>
    /// Unique instance Id
    /// </summary>
    long FactoryInstanceId { get; }

    ///// <summary>
    ///// Factory <see cref="IAccumulatingFactory"/> latest version
    ///// </summary>
    //long FactoryVersion { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="busBroker"></param>
    /// <returns></returns>
    Task StartAsync(IAsyncBusBroker busBroker);
  }

  /// <summary>
  /// Accumulating factory for <typeparamref name="T"/> objects
  /// </summary>
  /// <typeparam name="T">Type of accumulating object</typeparam>
  public interface IAccumulatingFactory<out T> : IAccumulatingFactory where T : ICloneable<T>, new()
  {
    /// <summary>
    /// Appling cumulative update <see cref="ICumulativeUpdate{T}"/> on top of specific version of accumulating object
    /// </summary>
    /// <param name="cumulativeUpdate">Cumulative update to apply</param>
    /// <returns>New version of the object or throw an exception</returns>
    Task<long> ApplyUpdateAsync(ICumulativeUpdate<T> cumulativeUpdate);
  }
}