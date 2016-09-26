using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace QX.Zoo.Talk.RabbitMQ
{
    public sealed class RabbitMQConfiguration
    {
        public List<string> HostNames { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Uri { get; set; }

        public RabbitMQConfiguration()
        {
            HostNames = new List<string>();
        }
    }
}
