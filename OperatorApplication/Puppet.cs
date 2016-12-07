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
    using TupleMessage = List<IList<string>>;

    internal partial class Operator : MarshalByRefObject, IPuppet {
        private enum LogStatus {
            FULL,
            LIGHT
        }

        #region Constants
        private const string PUPPET_SERVICE_NAME = "Puppet";
        #endregion
        #region Variables
        //Puppet variables
        private IPuppetMaster _puppetMaster;
        private LogStatus _logStatus;
        private int _sleepBetweenEvents;
		private string _state;

        //Frozen request variables
        private IProducerConsumerCollection<Message>                        _frozenInfrastructureRequests,
                                                                            _frozenDownstreamRequests,
                                                                            _frozenUpstreamRequests,

        //Frozen reply variables
                                                                            _frozenInfrastructureReplies;
        private IProducerConsumerCollection<TupleMessage>                   _frozenDownstreamReplies;
        private IProducerConsumerCollection<Process>                        _frozenUpstreamReplies;
        private IProducerConsumerCollection<Tuple<int, Process>>            _frozenEpochChangeReplies;
        private IProducerConsumerCollection<Tuple<TupleMessage, string>>    _frozenPaxosReplies;
        private IProducerConsumerCollection<TupleMessage>                   _frozenQuorumReplies;
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

            _puppetMaster.ReceiveUrl(_process.SuffixedUrl, objRef);
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

        private void FrozenInfrastructureReplyHandler(Message reply) {
            _frozenInfrastructureReplies.TryAdd(reply);
        }

        private void FrozenDownstreamReplyHandler(TupleMessage reply) {
            _frozenDownstreamReplies.TryAdd(reply);
        }

        private void FrozenUpstreamReplyHandler(Process reply) {
            _frozenUpstreamReplies.TryAdd(reply);
        }

        private void FrozenEpochChangeReplyHandler(int timestamp, Process process) {
            _frozenEpochChangeReplies.TryAdd(new Tuple<int, Process>(timestamp, process));
        }

        private void FrozenPaxosReplyHandler(Tuple<TupleMessage, string> reply) {
            _frozenPaxosReplies.TryAdd(reply);
        }

        private void FrozenQuorumReplyHandler(TupleMessage reply) {
            _frozenQuorumReplies.TryAdd(reply);
        }
        #endregion
        #region Commands
        public void Start() {
			_state = "running";

            //Process already received tuples
            Unfreeze();

            //Process files
            if (_serverType == ServerType.REPLICATION) {
                return;
            }
            foreach (StreamReader currentInputFile in _inputFiles) {
                ThreadPool.QueueUserWorkItem((inputFileObject) => {
                    StreamReader inputFile = (StreamReader)inputFileObject;

                    string currentLine;
                    while ((currentLine = inputFile.ReadLine()) != null) {
                        ThreadPool.QueueUserWorkItem((lineObject) => {
                            //Assumption: all files and lines are valid
                            string line = (string)lineObject;
                            TupleMessage tupleMessage = new TupleMessage();
                            tupleMessage.Add(line.Split(',').ToList());
                            Console.WriteLine("Reading " + string.Join(" , ", tupleMessage.Select(aa => string.Join("-", aa))));
                            UnfrozenDownstreamReplyHandler(tupleMessage);
                        }, (Object)currentLine);
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

            //Reset frozen request sets
            _frozenInfrastructureRequests   = new ConcurrentBag<Message>();
            _frozenDownstreamRequests       = new ConcurrentBag<Message>();
            _frozenUpstreamRequests         = new ConcurrentBag<Message>();

            //Reset frozen reply sets
            _frozenInfrastructureReplies    = new ConcurrentBag<Message>();
            _frozenDownstreamReplies        = new ConcurrentBag<TupleMessage>();
            _frozenUpstreamReplies          = new ConcurrentBag<Process>();
            _frozenEpochChangeReplies       = new ConcurrentBag<Tuple<int, Process>>();
            _frozenPaxosReplies             = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenQuorumReplies            = new ConcurrentBag<TupleMessage>();

            //Add frozen request handlers
            _infrastructureRequestListener  = FrozenInfrastructureRequestHandler;
            _downstreamRequestListener      = FrozenDownstreamRequestHandler;
            _upstreamRequestListener        = FrozenUpstreamRequestHandler;

            //Add frozen reply handlers
            _infrastructureReplyListener    = FrozenInfrastructureReplyHandler;
            _downstreamReplyListener        = FrozenDownstreamReplyHandler;
            _upstreamReplyListener          = FrozenUpstreamReplyHandler;
            _epochChangeReplyListener       = FrozenEpochChangeReplyHandler;
            _paxosReplyListener             = FrozenPaxosReplyHandler;
            _quorumReplyListener            = FrozenQuorumReplyHandler;
        }

        public void Unfreeze() {
            Flag.Frozen = false;
            _state = "running";

            //Add unfrozen request handlers
            _infrastructureRequestListener  = UnfrozenInfrastructureRequestHandler;
            _downstreamRequestListener      = UnfrozenDownstreamRequestHandler;
            _upstreamRequestListener        = UnfrozenUpstreamRequestHandler;

            //Add unfrozen reply handlers
            _infrastructureReplyListener    = UnfrozenInfrastructureReplyHandler;
            _downstreamReplyListener        = UnfrozenDownstreamReplyHandler;
            _upstreamReplyListener          = UnfrozenUpstreamReplyHandler;
            _epochChangeReplyListener       = UnfrozenEpochChangeReplyHandler;
            _paxosReplyListener             = UnfrozenPaxosReplyHandler;
            _quorumReplyListener            = UnfrozenQuorumReplyHandler;

            //Send frozen requests
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

            //Send frozen replies
            foreach (Message frozenInfrastructureReply in _frozenInfrastructureReplies) {
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
            foreach (Tuple<int, Process> frozenEpochChangeReply in _frozenEpochChangeReplies) {
                new Thread(() => {
                    _epochChangeReplyListener(frozenEpochChangeReply.Item1, frozenEpochChangeReply.Item2);
                }).Start();
            }
            foreach (Tuple<TupleMessage, string> frozenPaxosReply in _frozenPaxosReplies) {
                new Thread(() => {
                    _paxosReplyListener(frozenPaxosReply);
                }).Start();
            }
            foreach (TupleMessage frozenQuorumReply in _frozenQuorumReplies) {
                new Thread(() => {
                    _quorumReplyListener(frozenQuorumReply);
                }).Start();
            }

            //Reset frozen request sets
            _frozenInfrastructureRequests   = new ConcurrentBag<Message>();
            _frozenDownstreamRequests       = new ConcurrentBag<Message>();
            _frozenUpstreamRequests         = new ConcurrentBag<Message>();

            //Reset frozen reply sets
            _frozenInfrastructureReplies    = new ConcurrentBag<Message>();
            _frozenDownstreamReplies        = new ConcurrentBag<TupleMessage>();
            _frozenUpstreamReplies          = new ConcurrentBag<Process>();
            _frozenEpochChangeReplies       = new ConcurrentBag<Tuple<int, Process>>();
            _frozenPaxosReplies             = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenQuorumReplies            = new ConcurrentBag<TupleMessage>();
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

			if (count == 0) {
                Console.Write("\r\n");
            }
			Console.Write("\r\n");

			foreach (KeyValuePair<string,string> pair in _command.Status()) {
                Console.WriteLine("\t " + pair.Key + ": \t\t" + pair.Value);
            }
        }

        public void Interval(int milliseconds) {
            _sleepBetweenEvents = milliseconds;
        }

        public void Routing(string routing) {
            //TODO: by default and on first release, the semantic is at-most-once
        }

        public void Semantics(string semantics) {
            //TODO: by default and on first release, the semantic is at-most-once
        }

        public void LoggingLevel(string loggingLevel) {
            if (loggingLevel.Equals("full")) {
                _logStatus = LogStatus.FULL;
            } else if (loggingLevel.Equals("light")) {
                _logStatus = LogStatus.LIGHT;
            }
        }
        #endregion
        #region Log
        private void Log(LogStatus logStatus, string message) {
            if (logStatus >= _logStatus) {
                Task.Run(() => {
                    _puppetMaster.Log(string.Format(
                    "tuple {0} {1}",
                    _process.SuffixedUrl,
                    message));
                });
            }
        }
        #endregion
    }
}
