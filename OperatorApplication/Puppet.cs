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
		private string _state;

        public void SubmitAsPuppet() {
			_state = "ready";

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            //IDictionary RemoteChannelProperties = new Hashtable();
            //RemoteChannelProperties["name"] = PUPPET_SERVICE_NAME;
            //TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            //ChannelServices.RegisterChannel(channel, true);

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
			_state = "running";
            _waitHandle.Set();
        }

        public void Interval(Milliseconds milliseconds) {
            _sleepBetweenEvents = milliseconds;
        }

        public void Status() {
            Console.WriteLine("status:");
			Console.WriteLine("\t Operator type: \t" + _command.ToString());
			Console.WriteLine("\t State: \t\t" + _state);
			Console.WriteLine("\t Waiting interval: \t" + _sleepBetweenEvents);

			//TODO display more stuff in status?
			Console.Write("\r\n\t Recievers: \t");
			int count = 0;
			foreach (Process receiver in _outputReceivers) {
				Console.Write("\t" + receiver.ServiceName +"  at " + receiver.Url);
				Console.Write("\r\n\t\t\t");
				count++;
			}

			if (count == 0) { Console.Write("\r\n"); }
			Console.Write("\r\n");

			foreach (KeyValuePair<string,string> pair in _command.Status()) {
                Console.WriteLine("\t " + pair.Key + ": \t\t" + pair.Value);
            }

            //Log(_logStatus, _command.ToString());

        }

        public void Crash() {
            Environment.Exit(0);
        }

        public void Freeze() {
			_state = "freezed";
            _listener = StoreMessage;
            _send = StoreReply;
            Flag.Frozen = true;
        }

        public void Unfreeze() {
			_state = "running";
            LoadStoredMessages();
            Flag.Frozen = false;
        }

        public void Semantics(String semantics) {
            //TODO: by default and on first release, the semantic is at-most-once
        }

        public void LoggingLevel(String loggingLevel) {
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
