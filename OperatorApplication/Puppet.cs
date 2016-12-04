using CommonTypesLibrary;
using DistributedAlgoritmsClassLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace OperatorApplication
{
    using Message = Object;
    using TupleMessage = List<IList<String>>;

    internal partial class Operator : MarshalByRefObject, IPuppet {
        private enum LogStatus {
            FULL,
            LIGHT
        }

        #region Constants
        private const String PUPPET_SERVICE_NAME = "Puppet";
        #endregion
        #region Variables
        //Puppet variables
        private IPuppetMaster _puppetMaster;
        private LogStatus _logStatus;
        private int _sleepBetweenEvents;
		private string _state;

        //Broadcast variables
        private IProducerConsumerCollection<Message> _frozenInfrastructureRequests,
                                                     _frozenDownstreamRequests,
                                                     _frozenUpstreamRequests;
        private IProducerConsumerCollection<Tuple<RequestType, String>> _frozenInfrastructureReplies;
        private IProducerConsumerCollection<TupleMessage> _frozenDownstreamReplies;
        private IProducerConsumerCollection<Process> _frozenUpstreamReplies;
        #endregion

        #region Constructor
        public void SubmitOperatorAsPuppetryNode() {
			_state = "ready";

            ObjRef objRef = RemotingServices.Marshal(
                this,
                PUPPET_SERVICE_NAME,
                typeof(IPuppet));

            _puppetMaster = (IPuppetMaster)Activator.GetObject(
                typeof(IPuppetMaster),
                "tcp://localhost:10001/PuppetMaster");

            _puppetMaster.ReceiveUrl(_process.Url, objRef);
        }
        #endregion

        #region Frozen Handlers
        private void FrozenInfrastructureRequestHandler(Message request) {
            _frozenInfrastructureRequests.TryAdd(request);
        }

        private void FrozenDownstreamRequestHandler(Message request) {
            _frozenDownstreamRequests.TryAdd(request);
        }

        private void FrozenUpstreamRequestHandler(Message request) {
            _frozenUpstreamRequests.TryAdd(request);
        }

        private void FrozenInfrastructureReplyHandler(Tuple<RequestType, String> reply) {
            _frozenInfrastructureReplies.TryAdd(reply);
        }

        private void FrozenDownstreamReplyHandler(TupleMessage reply) {
            _frozenDownstreamReplies.TryAdd(reply);
        }

        private void FrozenUpstreamReplyHandler(Process reply) {
            _frozenUpstreamReplies.TryAdd(reply);
        }
        #endregion
        #region Commands
        public void Start() {
			_state = "running";

            //Process already received tuples
            Unfreeze();

            //Process files
            while (_serverType == ServerType.UNDEFINED) {
                Console.WriteLine("Warning: The server is still undefined.");
                Thread.Sleep(1000);
            }
            if (_serverType == ServerType.REPLICATION) {
                return;
            }
            foreach (StreamReader currentInputFile in _inputFiles) {
                ThreadPool.QueueUserWorkItem((inputFileObject) => {
                    StreamReader inputFile = (StreamReader)inputFileObject;

                    String currentLine;
                    while ((currentLine = inputFile.ReadLine()) != null) {
                        new Thread((lineObject) => {
                            //Assumption: all files and lines are valid
                            String line = (String)lineObject;
                            TupleMessage tupleMessage = new TupleMessage();
                            tupleMessage.Add(line.Split(',').ToList());
                            PaxosRequestHandler(tupleMessage);

                            Console.WriteLine("Reading " + String.Join(" , ", tupleMessage.Select(aa => String.Join("-", aa))));
                        }).Start((Object)currentLine);
                    }
                    inputFile.Close();
                }, (Object)currentInputFile);
            }
        }

        public void Crash() {
            Environment.Exit(0);
        }

        public void Freeze() {
            Flag.Frozen = true;
			_state = "froze";

            _frozenInfrastructureRequests = new ConcurrentBag<Message>();
            _frozenDownstreamRequests = new ConcurrentBag<Message>();
            _frozenUpstreamRequests = new ConcurrentBag<Message>();
            _frozenInfrastructureReplies = new ConcurrentBag<Tuple<RequestType, String>>();
            _frozenDownstreamReplies = new ConcurrentBag<TupleMessage>();
            _frozenUpstreamReplies = new ConcurrentBag<Process>();

            _infrastructureRequestListener = FrozenInfrastructureRequestHandler;
            _downstreamRequestListener = FrozenDownstreamRequestHandler;
            _upstreamRequestListener = FrozenUpstreamRequestHandler;
            _infrastructureReplyListener = FrozenInfrastructureReplyHandler;
            _downstreamReplyListener = FrozenDownstreamReplyHandler;
            _upstreamReplyListener = FrozenUpstreamReplyHandler;
        }

        public void Unfreeze() {
            Flag.Frozen = false;
            _state = "running";

            _infrastructureRequestListener = _infrastructureBroadcast.Broadcast;
            _downstreamRequestListener = _downstreamBroadcast.Broadcast;
            _upstreamRequestListener = _upstreamBroadcast.Broadcast;
            _infrastructureReplyListener = InitHandler;
            _downstreamReplyListener = PaxosRequestHandler;
            _upstreamReplyListener = _downstreamBroadcast.Connect;

            foreach (Message frozenInfrastructureRequest in _frozenInfrastructureRequests) {
                new Thread(() => {
                    _infrastructureRequestListener(frozenInfrastructureRequest);
                }).Start();
            }
            foreach (Message frozenDownstreamRequest in _frozenDownstreamRequests) {
                new Thread(() => {
                    _downstreamRequestListener(frozenDownstreamRequest);
                }).Start();
            }
            foreach (Message frozenUpstreamRequest in _frozenUpstreamRequests) {
                new Thread(() => {
                    _upstreamRequestListener(frozenUpstreamRequest);
                }).Start();
            }
            foreach (Tuple<RequestType, String> frozenInfrastructureReply in _frozenInfrastructureReplies) {
                new Thread(() => {
                    _infrastructureReplyListener(frozenInfrastructureReply);
                }).Start();
            }
            foreach (TupleMessage frozenDownstreamReply in _frozenDownstreamReplies) {
                new Thread(() => {
                    _downstreamReplyListener(frozenDownstreamReply);
                }).Start();
            }
            foreach (Process frozenUpstreamReply in _frozenUpstreamReplies) {
                new Thread(() => {
                    _upstreamReplyListener(frozenUpstreamReply);
                }).Start();
            }

            _frozenInfrastructureRequests = new ConcurrentBag<Message>();
            _frozenDownstreamRequests = new ConcurrentBag<Message>();
            _frozenUpstreamRequests = new ConcurrentBag<Message>();
            _frozenInfrastructureReplies = new ConcurrentBag<Tuple<RequestType, String>>();
            _frozenDownstreamReplies = new ConcurrentBag<TupleMessage>();
            _frozenUpstreamReplies = new ConcurrentBag<Process>();
        }

        public void Status() {
            Console.WriteLine("status:");
			Console.WriteLine("\t Operator type: \t" + _command.ToString());
			Console.WriteLine("\t State: \t\t" + _state);
			Console.WriteLine("\t Waiting interval: \t" + _sleepBetweenEvents);

			//TODO display more stuff in status?
			Console.Write("\r\n\t Recievers: \t");
			int count = 0;
			foreach (Process receiver in _downstreamBroadcast.Processes) {
				Console.Write("\t" + receiver.ServiceName +"  at " + receiver.Url);
				Console.Write("\r\n\t\t\t");
				count++;
			}

			if (count == 0) { Console.Write("\r\n"); }
			Console.Write("\r\n");

			foreach (KeyValuePair<string,string> pair in _command.Status()) {
                Console.WriteLine("\t " + pair.Key + ": \t\t" + pair.Value);
            }
        }

        public void Interval(int milliseconds) {
            _sleepBetweenEvents = milliseconds;
        }

        public void Routing(String routing) {
            //TODO: by default and on first release, the semantic is at-most-once
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
        #endregion
        #region Log
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
        #endregion
    }
}
