namespace QX.Zoo.Accumulative.Exceptions
{
  public class InvalidSnapshotStateException : AccumulatingFactoryException
  {
    public InvalidSnapshotStateException(string message) : base(message)
    { }
  }
}