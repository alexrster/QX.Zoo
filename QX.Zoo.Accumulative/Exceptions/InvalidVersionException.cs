namespace QX.Zoo.Accumulative.Exceptions
{
  public class InvalidVersionException : InvalidSnapshotStateException
  {
    public InvalidVersionException(string message) : base(message)
    { }
  }
}