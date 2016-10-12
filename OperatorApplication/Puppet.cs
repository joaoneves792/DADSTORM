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
        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenRequests, _frozenReplies;

        private int _sleepBetweenEvents;

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
            _sleepBetweenEvents = milliseconds;
        }

        public void Status() {
            Console.WriteLine("STATUS");

            //TODO: Implement me
            // We need to display status of the rest of the system from our point of view
            //Cant do that right now
        }

        public void Crash()
        {
            Console.WriteLine("CRASH");
            Environment.FailFast("Crash simulation");
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE!");

            _listener = StoreMessage;
            _send = StoreReply;

            Log.WriteLine(LogStatus.DEBUG, "FREEZE!");
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE!");

            Log.WriteLine(LogStatus.DEBUG, "UNFREEZE!");

            LoadStoredMessages();
        }
    }
}
