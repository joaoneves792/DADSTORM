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
using CommonTypesLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using Milliseconds = Int32;

    internal partial class Operator : MarshalByRefObject, IPuppet
    {
        private enum LogStatus {
            FULL,
            LIGHT
        }

        private static String PUPPET_SERVICE_NAME = "Puppet";
        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenRequests, _frozenReplies;
        private IPuppetMaster _puppetMaster;
        private LogStatus _logStatus;

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

            _puppetMaster = (IPuppetMaster)Activator.GetObject(
                typeof(IPuppetMaster),
                "tcp://localhost:10001/PuppetMaster");

            _puppetMaster.ReceiveUrl(_process.Url, objRef);
        }

        public void Start() {
            _waitHandle.Set();
        }

        public void Interval(Milliseconds milliseconds) {
            _sleepBetweenEvents = milliseconds;
        }

        public void Status() {
			//TODO: Implement me
			// We need to display status of the rest of the system from our point of view
			//Cant do that right now

			Console.WriteLine("sTATUS :");
			Console.WriteLine("\t Operator type : " + _command.ToString());
			Console.WriteLine("\t State: " + "running");

			foreach (KeyValuePair<string,string> pair in _command.Status()) {
                Console.WriteLine("\t " + pair.Key + ": " + pair.Value);
            }

            Log(_logStatus, _command.ToString());

        }

        public void Crash()
        {
            Environment.Exit(0);
        }

        public void Freeze()
        {
            _listener = StoreMessage;
            _send = StoreReply;
        }

        public void Unfreeze()
        {
            LoadStoredMessages();
        }

        public void Semantics(String semantics)
        {
            //TODO: by default and on first release, the semantic is at-most-once
        }

        public void LoggingLevel(String loggingLevel)
        {
            if (loggingLevel.Equals("full")) {
                _logStatus = LogStatus.FULL;
            } else if (loggingLevel.Equals("light")) {
                _logStatus = LogStatus.LIGHT;
            }
        }

        private void Log(LogStatus logStatus, String message) {
            if (logStatus >= _logStatus) {
                Task.Run(() => {
                    _puppetMaster.Log(String.Format(
                    "tuple {0} {1}",
                    _process.Url,
                    message));
                });
            }
        }
    }
}
