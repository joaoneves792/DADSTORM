using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;

using DistributedAlgoritmsClassLibrary;
using LoggingClassLibrary;
using CommonTypesLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using Milliseconds = Int32;

    internal partial class Operator : MarshalByRefObject, IPuppet
    {
        private static String PUPPET_SERVICE_NAME = "Puppet";
        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenRequests;

        public void SubmitAsPuppet()
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            /*IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["name"] = PUPPET_SERVICE_NAME;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);*/

            ObjRef objRef = RemotingServices.Marshal(
                this,
                PUPPET_SERVICE_NAME,
                typeof(IPuppet));

            IPuppetMaster puppetMaster = (IPuppetMaster)Activator.GetObject(
                typeof(IPuppetMaster),
                "tcp://localhost:10001/PuppetMaster");

            puppetMaster.ReceiveUrl(_process.Url, objRef);
        }

        public void Start() {
            Console.WriteLine("START");
            _waitHandle.Set();
        }

        public void Interval(Milliseconds milliseconds) {
            Console.WriteLine("INTERVAL");

            //TODO: Implement me
        }

        public void Status() {
            Console.WriteLine("STATUS");

            //TODO: Implement me
        }

        public void Crash()
        {
            Console.WriteLine("CRASH");
            Environment.FailFast("Crash simulation");
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE!");

            _listener = (process, message) => {
                Tuple<Process, Message> request = new Tuple<Process, Message>(process, message);
                while (!_frozenRequests.TryAdd(request))
                {
                    Log.WriteLine(LogStatus.CRITICAL, "Message not froze");
                }
                Log.WriteLine(LogStatus.DEBUG, _frozenRequests.Count + " frozen requests");
            };

            Log.WriteLine(LogStatus.DEBUG, "FREEZE!");
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE!");

            Log.WriteLine(LogStatus.DEBUG, "UNFREEZE!");

            _listener = ParseMessage;

            foreach (Tuple<Process, Message> request in _frozenRequests)
            {
                Task.Run(() => {
                    _listener(request.Item1, request.Item2);
                });
            }
            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
        }
    }
}
