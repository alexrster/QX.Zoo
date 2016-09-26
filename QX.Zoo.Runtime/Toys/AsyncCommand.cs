using System;

namespace QX.Zoo.Runtime.Toys
{
  public abstract class AsyncCommand
  {
    private static readonly IGuidGenerator GuidGenerator = SequentialGuidGenerator.GetOrCreateGenerator<AsyncCommand>();

    public Guid CommandId { get; set; }
    public string RecepientId { get; set; }

    protected AsyncCommand()
    {
      CommandId = GuidGenerator.NewGuid();
    }
  }
}
