using System;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public class VersionMoveAndConfirmNotification : VersionConfirmNotification
  {
    public long OldVersion { get; set; }

    public VersionMoveAndConfirmNotification()
    { }

    public VersionMoveAndConfirmNotification(Guid factoryId, long factoryInstanceId, long versionNumber, long baseVersion, long oldVersion)
      : base(factoryId, factoryInstanceId, versionNumber, baseVersion)
    {
      OldVersion = oldVersion;
    }
  }
}
