using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using QX.Zoo.Runtime.Scopes;

namespace QX.Zoo.Runtime.Logging
{
  public static class ZooLoggerExtensions
  {
    public static ILoggerFactory AddZooLogger(this ILoggerFactory loggerFactory, IOperationScopeProvider operationScopeProvider, IConfiguration config)
    {
      var settings = new ConfigurationConsoleLoggerSettings(config);

      loggerFactory.AddProvider(new ZooLoggerProvider(operationScopeProvider, (s, level) => true));

      return loggerFactory;
    }

    public static void LogVerbose(this ILogger logger, string format, params object[] args)
    {
      logger.LogTrace(format, args);
    }
  }
}