using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace ProcessCreationServiceApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int PORT = 1000;

            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ProcessCreationService), "ProcessCreationService", WellKnownObjectMode.Singleton);

            Console.WriteLine("PCS waiting for commands...");
            Console.ReadKey();

        }
    }
}
