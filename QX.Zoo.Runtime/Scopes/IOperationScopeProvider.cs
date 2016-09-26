namespace QX.Zoo.Runtime.Scopes
{
  public interface IOperationScopeProvider
  {
    IOperationScope Current { get; }
  }
}