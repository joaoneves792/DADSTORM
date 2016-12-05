using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.IO;

using CommonTypesLibrary;

namespace TestingConsoleApplication
{
    class Program : MarshalByRefObject
    {
        static void Main(string[] args)
        {
            //new Test();

            //test1();
            test2();
            //test3();
            //test4();
        }

        static void test1()
        {
            string arguments;

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
        }

        static void test2()
        {
            Process.Start("ProcessCreationServiceApplication.exe");
            Process.Start("PuppetMasterForm.exe");
        }

        static void test3()
        {
            Process.Start("OperatorApplication.exe", "1 tcp://localhost:55003/teste Scripts/1.txt none DUP");

            /*if (args[0].Equals("Teste1"))
            {
                node.Freeze();
                Thread.Sleep(20000);
                node.Unfreeze();
            }*/
        }

        static void test4() {
            string[] inputOpsList = {"Scripts/1.txt", "Scripts/2.txt", "Scripts/3.txt", "Scripts/4.txt" };

            FileStream fileStream;
            ICollection<StreamReader> inputFiles = new HashSet<StreamReader>();
            foreach (string inputOp in inputOpsList)
            {
                Console.WriteLine("Identified path " + inputOp);

                fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                inputFiles.Add(new StreamReader(fileStream));
            }

            //Process files
            foreach (StreamReader currentInputFile in inputFiles)
            {
                new Thread(() => {
                    StreamReader inputFile = currentInputFile;

                    string line;
                    while ((line = inputFile.ReadLine()) != null)
                    {
                        new Thread(() => {
                            Console.WriteLine(line);
                        }).Start();
                    }
                    inputFile.Close();
                }).Start();
            }

            Console.ReadLine();
        }
    }

    internal class Test : MarshalByRefObject//, IPuppetMaster
    {
        internal Test() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["name"] = "PuppetMaster";
            RemoteChannelProperties["port"] = 10001;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(
                this,
                "PuppetMaster",
                typeof(IPuppetMaster));
        }

        public void ReceiveUrl(string url, ObjRef objRef)
        {
            TestFreeze(objRef);
        }

        private void TestFreeze(ObjRef objRef)
        {
            IPuppet puppet = (IPuppet) RemotingServices.Unmarshal(objRef);

            puppet.Freeze();
        }
    }
}
