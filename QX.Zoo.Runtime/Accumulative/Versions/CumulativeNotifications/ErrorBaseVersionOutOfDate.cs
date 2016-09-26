using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public class ErrorBaseVersionOutOfDate : FactoryNotification
  {
    public ErrorBaseVersionOutOfDate()
    { }

    public ErrorBaseVersionOutOfDate(Guid factoryId, long factoryInstanceId) : base(factoryId, factoryInstanceId)
    { }
  }
}
