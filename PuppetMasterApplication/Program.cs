using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using ProcessCreationServiceApplication;

namespace PuppetMasterApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Replace this with the actual PM logic
            int port = 1024;
            String[] replicas_urls = { "" };
            while (true)
            {
                Console.WriteLine("Press a key to create a new Operator");
                Console.ReadKey();
                port++;
                createNewOperator("localhost", 1000, port, replicas_urls);
            }

        }

        private static void createNewOperator(String pcsHost, int pcsPort, int operatorPort, String[] replica_urls)
        {
            String URL = "tcp://" + pcsHost + ":" + pcsPort + "/ProcessCreationService";

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            ProcessCreationService pcs = (ProcessCreationService)Activator.GetObject(typeof(ProcessCreationService), URL);

            Console.WriteLine("Creating a new Operator...");
            pcs.createProcess(operatorPort, replica_urls);
     
            ChannelServices.UnregisterChannel(channel);
        }
    }
}
