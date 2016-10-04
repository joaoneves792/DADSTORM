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

using LoggingClassLibrary;
using CommonTypesLibrary;

namespace TestingConsoleApplication
{
    class Program : MarshalByRefObject
    {
        static void Main(string[] args)
        {
            Log.LogStatus = LogStatus.DEBUG;

            new Test();

            //test1();
            //test2();
            test3();

            Console.ReadLine();
        }

        static void test1()
        {
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
        }

        static void test2()
        {
            Process.Start("PuppetMasterApplication.exe");
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
    }

    internal class Test : MarshalByRefObject, IPuppetMaster
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

        public void ReceiveUrl(ObjRef objRef)
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
