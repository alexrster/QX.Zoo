using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public class VersionConfirmNotification : VersionNotification
  {
    public long BaseVersionNumber { get; set; }

    public VersionConfirmNotification()
    { }

    public VersionConfirmNotification(Guid factoryId, long factoryInstanceId, long versionNumber, long baseVersionNumber)
      : base(factoryId, factoryInstanceId, versionNumber)
    {
      BaseVersionNumber = baseVersionNumber;
    }
  }
}
