using System;
using Microsoft.Extensions.Logging;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Runtime.Logging
{
  class ZooLogger : ILogger
  {
    private readonly ILogger _logger;
    private readonly IOperationScopeProvider _operationScopeProvider;

    public string CategoryName { get; }

    public ZooLogger(ILoggerFactory loggerFactory, IOperationScopeProvider operationScopeProvider, string categoryName)
      : this(loggerFactory.CreateLogger(categoryName), operationScopeProvider, categoryName)
    { }

    public ZooLogger(ILogger logger, IOperationScopeProvider operationScopeProvider, string categoryName)
    {
      _logger = logger;
      _operationScopeProvider = operationScopeProvider;

      CategoryName = categoryName;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return _logger.IsEnabled(logLevel);
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return _logger.BeginScope(state);
    }
  }
}
