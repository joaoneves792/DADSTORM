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

            String arguments;

            arguments = "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste2" + " " + "tcp://localhost:54002/teste" + " " +
                        "Teste3" + " " + "tcp://localhost:55003/teste";
            Process.Start("SecondaryConsoleApplication.exe", arguments);

            arguments = "Teste2" + " " + "tcp://localhost:54002/teste" + " " +
                        "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste3" + " " + "tcp://localhost:55003/teste";
            Process.Start("SecondaryConsoleApplication.exe", arguments);

            arguments = "Teste3" + " " + "tcp://localhost:55003/teste" + " " +
                        "Teste1" + " " + "tcp://localhost:53001/teste" + " " +
                        "Teste2" + " " + "tcp://localhost:54002/teste";
            Process.Start("SecondaryConsoleApplication.exe", arguments);

            //Process.Start("PuppetMasterApplication.exe");
        }
    }
}
