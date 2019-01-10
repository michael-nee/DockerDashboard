using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalrDashboardDemo.Options
{
    public class DockerOptions
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 2375;

        public string ConnectionString { get { return Host + ":" + Port; } }
    }
}
