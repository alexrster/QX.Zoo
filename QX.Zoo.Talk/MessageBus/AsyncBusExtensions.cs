using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QX.Zoo.Talk.MessageBus
{
  public static class AsyncBusExtensions
  {
    private static readonly Task DefaultResultTask = Task.FromResult(0);

    public static Task PublishMessageAsync<T>(this IAsyncBusEntity entity, T message) where T : class
    {
      return entity.PublishMessageAsync(new Dictionary<string, string>(), message);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<IAsyncBusEntity, string, IDictionary<string, string>, T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler(s, i, h, (T)d) : DefaultResultTask);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<IAsyncBusEntity, IDictionary<string, string>, T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler(s, h, (T)d) : DefaultResultTask);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<IDictionary<string, string>, T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler(h, (T)d) : DefaultResultTask);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<IAsyncBusEntity, string, T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler(s, i, (T)d) : DefaultResultTask);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<IAsyncBusEntity, T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler(s, (T)d) : DefaultResultTask);
    }

    public static Task<IAsyncBusEntitySubscription> SubscribeAsync<T>(this IAsyncBusEntity entity, Func<T, Task> messageHandler) where T : class
    {
      return entity.SubscribeAsync((s, i, h, d) => d is T ? messageHandler((T)d) : DefaultResultTask);
    }
  }
}
