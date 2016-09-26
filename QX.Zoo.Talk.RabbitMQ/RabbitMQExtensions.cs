using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace QX.Zoo.Talk.RabbitMQ
{
    public static class RabbitMQExtensions
    {
        public static IConfigurationBuilder AddRabbitDefaults(this IConfigurationBuilder config)
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("uri", "amqp://guest:guest@localhost:5672")
            });
            return config;
        }

        public static IConfigurationBuilder AddRabbitJsonFile(this IConfigurationBuilder config)
        {
            config.AddJsonFile("rabbit.json");
            return config;
        }

        public static RabbitMQConfiguration GetRabbitMQConfiguration(this IConfiguration section)
        {
            try
            {
                return new RabbitMQConfiguration
                {
                    HostNames = new List<string>(new[] { section["RabbitMQ:hostnames:0"] }),
                    UserName = section["RabbitMQ:username"],
                    Password = section["RabbitMQ:password"],
                    Port = int.Parse(section["RabbitMQ:port"] ?? "5672")
                };
            }
            catch (Exception e)
            {
                Trace.TraceError("Cannot read RabbitMQ configuration section: {0}", e);
            }

            return null;
        }
    }
}
