namespace QX.Zoo.Talk.MessageBus
{
  /// <summary>
  /// QX asynchronous bus
  /// </summary>
  public interface IAsyncBusBroker
  {
    /// <summary>
    /// Get QX bus entity
    /// </summary>
    /// <param name="entityId">Entity URI</param>
    /// <returns>QX bus entity</returns>
    IAsyncBusEntity GetEntity(string entityId);
  }
}