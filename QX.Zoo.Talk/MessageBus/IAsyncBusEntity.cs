using System.Collections.Generic;
using System.Threading.Tasks;

namespace QX.Zoo.Talk.MessageBus
{
    /// <summary>
    /// QX bus entity message delegate
    /// </summary>
    /// <param name="entity">QX bus entity</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="headers">Headers collection</param>
    /// <param name="data">Payload</param>
    /// <returns>Handler asynchronous task</returns>
    public delegate Task AsyncBusHandlerDelegate(IAsyncBusEntity entity, string messageId, IDictionary<string, string> headers, object data);

    /// <summary>
    /// QX bus entity
    /// </summary>
    public interface IAsyncBusEntity
    {
        /// <summary>
        /// Unique QX bus entity ID
        /// </summary>
        string EntityId { get; }

        /// <summary>
        /// Publish message to QX bus entity
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="headers">Message metadata</param>
        /// <param name="message">Message data</param>
        /// <returns>Asynchronous send completion task with Message ID</returns>
        Task<string> PublishMessageAsync<T>(IDictionary<string, string> headers, T message) where T : class;

        /// <summary>
        /// Subscribe to QX bus entity
        /// </summary>
        /// <param name="asyncHandler">Handler</param>
        /// <returns>Subscription object</returns>
        Task<IAsyncBusEntitySubscription> SubscribeAsync(AsyncBusHandlerDelegate asyncHandler);
    }
}
