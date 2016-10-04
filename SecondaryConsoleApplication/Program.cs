using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DistributedAlgoritmsClassLibrary;
using LoggingClassLibrary;

namespace SecondaryConsoleApplication
{
    using Message = Object;

    class Program
    {
        static void Main(string[] args)
        {
            Log.LogStatus = LogStatus.DEBUG;

            Listener listener = new Listener();

            Process process1 = new Process(args[0], args[1]),
                    process2 = new Process(args[2], args[3]),
                    process3 = new Process(args[4], args[5]);

            if (args[0].Equals("Teste1"))
            {
                Thread.Sleep(20000);
            }

            RetransmitForever node = new RetransmitForever(process1, listener.Deliver/*, process2, process3*/);

            node.Connect(process2);

            if (!args[0].Equals("Teste1"))
            {
                Thread.Sleep(20000);
            }

            node.Connect(process3);

            Thread.Sleep(1000);

            node.Send(process2, "to " + process2.Name);

            Console.ReadLine();
        }

        internal class Listener
        {
            internal void Deliver(Process process, Message message)
            {
                Console.WriteLine("From " + process.Name + " " + (String)message);
            }
        }
    }
}
