using System;

namespace QX.Zoo.Runtime.Toys
{
  public abstract class AsyncNotification
  {
    private static readonly IGuidGenerator GuidGenerator = SequentialGuidGenerator.GetOrCreateGenerator<AsyncNotification>();

    public Guid NotificationId { get; private set; }

    protected AsyncNotification()
        : this(GuidGenerator.NewGuid())
    { }

    protected AsyncNotification(Guid notificationId)
    {
      NotificationId = notificationId;
    }

    protected static Guid NewGuid()
    {
      return GuidGenerator.NewGuid();
    }
  }
}
