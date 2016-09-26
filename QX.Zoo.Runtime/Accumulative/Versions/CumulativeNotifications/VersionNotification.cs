using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public abstract class VersionNotification : FactoryAsyncNotification
  {
    public long VersionNumber { get; set; }

    public VersionNotification()
    { }

    protected VersionNotification(Guid factoryId, long factoryInstanceId, long versionNumber)
      : base(factoryId, factoryInstanceId)
    {
      VersionNumber = versionNumber;
    }
  }
}
