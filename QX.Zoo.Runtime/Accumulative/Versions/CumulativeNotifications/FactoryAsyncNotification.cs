using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public class FactoryAsyncNotification : FactoryNotification
  {
    public Guid NotificationId { get; set; }

    public FactoryAsyncNotification()
    { }

    protected FactoryAsyncNotification(Guid factoryId, long factoryInstanceId) : base(factoryId, factoryInstanceId)
    {
      NotificationId = Guid.NewGuid();
    }
  }
}