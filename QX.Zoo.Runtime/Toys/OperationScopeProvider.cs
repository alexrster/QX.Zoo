using System;
using System.Threading;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Runtime.Toys
{
  public sealed class OperationScopeProvider : IOperationScopeProvider, IDisposable
  {
    private readonly AsyncLocal<IOperationScope> _localOperationScope = new AsyncLocal<IOperationScope>();

    public IOperationScope Current => _localOperationScope.Value;

    internal IOperationScope BeginChildScope(string name, Action<IOperationScope> configAction = null)
    {
      var newScope = new LocalOperationScope(this, Current, name);
      configAction?.Invoke(newScope);

      return newScope;
    }

    internal IOperationScope EndScope(LocalOperationScope scope)
    {
      if (!Equals(scope, Current))
      {
        throw new InvalidOperationException($"Cannot finilize scope '{scope}' before finalizing his child scopes");
      }

      _localOperationScope.Value = scope.ParentOperationScope;
      return _localOperationScope.Value;
    }

    public void Dispose()
    {
      do _localOperationScope.Value = (Current as LocalOperationScope)?.Dispose(); while (_localOperationScope.Value != null);
    }
  }
}
