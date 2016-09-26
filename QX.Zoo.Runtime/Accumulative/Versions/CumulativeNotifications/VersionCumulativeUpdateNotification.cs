using System;
using QX.Zoo.Accumulative;
using QX.Zoo.Hold;

namespace QX.Zoo.Runtime.Accumulative.Versions.CumulativeNotifications
{
  public class VersionCumulativeUpdateNotification<T> : VersionNotification
    where T : ICloneable<T>, new()
  {
    public ICumulativeUpdate<T> CumulativeUpdate { get; set; }

    public VersionCumulativeUpdateNotification()
    { }

    public VersionCumulativeUpdateNotification(Guid factoryId, long factoryInstanceId, long versionNumber, ICumulativeUpdate<T> update)
      : base(factoryId, factoryInstanceId, versionNumber)
    {
      CumulativeUpdate = update;
    }
  }
}
