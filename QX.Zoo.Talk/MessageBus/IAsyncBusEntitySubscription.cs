using System.Threading.Tasks;

namespace QX.Zoo.Talk.MessageBus
{
  /// <summary>
  /// Subscription to QX bus entity
  /// </summary>
  public interface IAsyncBusEntitySubscription
  {
    /// <summary>
    /// Unique QX bus entity subscription ID
    /// </summary>
    string SubscriptionId { get; }

    /// <summary>
    /// Unique QX bus entity ID
    /// </summary>
    string EntityId { get; }

    /// <summary>
    /// Cancel subscription for QX bus entity messages
    /// </summary>
    /// <returns>Asynchornous cancellation completion task</returns>
    Task CancelAsync();
  }
}
