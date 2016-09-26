using System;

namespace QX.Zoo.Runtime.Scopes
{
  public interface IOperationScope : IDisposable
  {
    T GetOrAdd<T>(string key, Func<string, T> valueFactory);
    T AddOrUpdate<T>(string key, Func<string, T> newValueFactory, Func<string, T, T> updateFunc);

    IOperationScope BeginScope(string name, Action<IOperationScope> configAction = null);
  }
}
