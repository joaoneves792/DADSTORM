using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ProcessCreationServiceApplication
{
    public class ProcessCreationService : MarshalByRefObject, IProcessCreationService
    {
        private static int PORT = 10000;
        public static string SERVICE_NAME = "ProcessCreationService";

        internal void Run() {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                this,
                SERVICE_NAME,
                typeof(IProcessCreationService));
        }

        public void CreateProcess(string arguments) {
            Process.Start("OperatorApplication.exe", arguments);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ProcessCreationService processCreatinService = new ProcessCreationService();
            processCreatinService.Run();

            Console.WriteLine("PCS waiting for commands...");

            Console.ReadKey();
        }
    }
}
