using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Runtime.Logging
{
  class ZooLoggerProvider : ILoggerProvider
  {
    private readonly IOperationScopeProvider _operationScopeProvider;
    private readonly Func<string, LogLevel, bool> _filterFunc;

    public ZooLoggerProvider(IOperationScopeProvider operationScopeProvider, Func<string, LogLevel, bool> filterFunc)
    {
      _operationScopeProvider = operationScopeProvider;
      _filterFunc = filterFunc;
    }

    public ILogger CreateLogger(string categoryName)
    {
      return new ZooLogger(new ConsoleLogger(categoryName, _filterFunc, false), _operationScopeProvider, categoryName);
    }

    public void Dispose()
    { }
  }
}