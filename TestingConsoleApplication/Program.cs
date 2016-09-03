using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LoggingClassLibrary;

namespace TestingConsoleApplication
{
    using Message = Object;

    class Program
    {
        static void Main(string[] args)
        {
            Log.LogStatus = LogStatus.DEBUG;

            //Listener listener = new Listener();

            /*Process process1 = new Process("Teste1", "tcp://localhost:53001/teste"),
                    process2 = new Process("Teste2", "tcp://localhost:54002/teste"),
                    process3 = new Process("Teste3", "tcp://localhost:55003/teste");*/

            String arguments;
            Process process;

            arguments = "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste2" + " " + "tcp://localhost:54002/teste" + " " +
                        "Teste3" + " " + "tcp://localhost:55003/teste";
            process = Process.Start("SecondaryConsoleApplication.exe", arguments);

            arguments = "Teste2" + " " + "tcp://localhost:54002/teste" + " " +
                        "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste3" + " " + "tcp://localhost:55003/teste";
            process = Process.Start("SecondaryConsoleApplication.exe", arguments);

            arguments = "Teste3" + " " + "tcp://localhost:55003/teste" + " " +
                        "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste2" + " " + "tcp://localhost:54002/teste";
            process = Process.Start("SecondaryConsoleApplication.exe", arguments);


            /*RmiNode node1 = new RmiNode(process1, listener.Deliver, process2, process3),
                    node2 = new RmiNode(process2, listener.Deliver, process1, process3),
                    node3 = new RmiNode(process3, listener.Deliver, process1, process2);

            node1.Send(process2, "to process 2");*/
        }

        /*internal class Listener
        {
            internal void Deliver(Process process, Message message)
            {
                System.Console.Write("From " + process.Name + " " + (String) message);
            }
        }*/
    }
}
