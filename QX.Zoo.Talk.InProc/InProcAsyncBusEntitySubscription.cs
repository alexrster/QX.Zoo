using System;
using System.Threading.Tasks;
using QX.Zoo.Talk.MessageBus;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace QX.Zoo.Talk.InProc
{
  class InProcAsyncBusEntitySubscription : IAsyncBusEntitySubscription
  {
    private static int _subscriptionNumber;

    private readonly Func<string, Task> _cancellationFunc;

    public string SubscriptionId { get; }
    public string EntityId { get; }

    public InProcAsyncBusEntitySubscription(string entityId, Func<string, Task> cancellationFunc, ILogger log)
    {
      _cancellationFunc = cancellationFunc;
      SubscriptionId = Interlocked.Increment(ref _subscriptionNumber).ToString("D4");
      EntityId = entityId;

      log.LogInformation($"Create InProcBus subscription '{SubscriptionId}' on entity '{EntityId}'");
    }

    public Task CancelAsync()
    {
      return _cancellationFunc(SubscriptionId);
    }
  }
}
