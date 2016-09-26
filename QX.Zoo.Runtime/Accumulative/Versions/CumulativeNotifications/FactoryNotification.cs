using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public abstract class FactoryNotification
  {
    public Guid FactoryId { get; set; }
    public long FactoryInstanceId { get; set; }

    public FactoryNotification()
    { }

    protected FactoryNotification(Guid factoryId, long factoryInstanceId)
    {
      FactoryId = factoryId;
      FactoryInstanceId = factoryInstanceId;
    }
  }
}
