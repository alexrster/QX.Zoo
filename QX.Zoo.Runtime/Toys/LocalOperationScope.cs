using System;
using System.Collections.Concurrent;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Runtime.Toys
{
  class LocalOperationScope : IOperationScope
  {
    private readonly OperationScopeProvider _operationScopeProvider;
    private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

    public Guid Id => (Guid) _data[@"$operationScopeId"];
    public string Name => (string) _data[@"$operationScopeName"];
    public IOperationScope ParentOperationScope => (IOperationScope) _data[@"$parentOperationScope"];

    public LocalOperationScope(OperationScopeProvider operationScopeProvider, IOperationScope parentOperationScope, string name)
    {
      _operationScopeProvider = operationScopeProvider;

      _data[@"$operationScopeId"] = Guid.NewGuid();
      _data[@"$operationScopeName"] = name;
      _data[@"$parentOperationScope"] = parentOperationScope;
    }

    public T GetOrAdd<T>(string key, Func<string, T> valueFactory)
      => (T)_data.GetOrAdd(key, k => valueFactory(k));

    public T AddOrUpdate<T>(string key, Func<string, T> newValueFactory, Func<string, T, T> updateFunc)
      => (T)_data.AddOrUpdate(key, k => newValueFactory(k), (k, v) => updateFunc(k, (T)v));

    public IOperationScope BeginScope(string name, Action<IOperationScope> configAction = null)
      => _operationScopeProvider.BeginChildScope(name, configAction);

    public IOperationScope Dispose()
      => _operationScopeProvider.EndScope(this);

    public override bool Equals(object obj)
      => (obj as LocalOperationScope)?.Id == Id;

    public override int GetHashCode()
      => base.GetHashCode();

    public override string ToString()
      => Name;

    void IDisposable.Dispose()
      => Dispose();
  }
}
