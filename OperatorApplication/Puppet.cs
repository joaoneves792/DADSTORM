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
        private IProducerConsumerCollection<Tuple<TupleMessage, string>>    _frozenDownstreamReplies;
        private IProducerConsumerCollection<Process>                        _frozenUpstreamReplies;
        private IProducerConsumerCollection<Tuple<int, Process>>            _frozenEpochChangeReplies;
        private IProducerConsumerCollection<Tuple<TupleMessage, string>>    _frozenPaxosReplies;
        private IProducerConsumerCollection<Tuple<TupleMessage, string>>    _frozenQuorumReplies;
        #endregion

        #region Constructor
        public void SubmitOperatorAsPuppetryNode(string loggingLevel) {
			_state = "ready";

            if (loggingLevel.Equals("full")) {
                _logStatus = LogStatus.FULL;
            } else if (loggingLevel.Equals("light")) {
                _logStatus = LogStatus.LIGHT;
            }

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

        private void FrozenDownstreamReplyHandler(Tuple<TupleMessage, string> reply) {
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

        private void FrozenQuorumReplyHandler(Tuple<TupleMessage, string> reply) {
            _frozenQuorumReplies.TryAdd(reply);
        }
        #endregion
        #region Commands
        public void Start() {
            //Save current server type before unfreezing operator
            ServerType serverType = _serverType;

            _state = "running";

            //Process already received tuples
            Unfreeze();

            //Process files
            if (serverType == ServerType.REPLICATION) {
                return;
            }
            Parallel.ForEach(_inputFiles, inputFile => {
                Parallel.ForEach(File.ReadLines(inputFile), (line, _, lineNumber) => {
                    TupleMessage tupleMessage = new TupleMessage();
                    tupleMessage.Add(line.Split(',').ToList());
                    //DEBUG: Console.WriteLine("Reading " + string.Join(" , ", tupleMessage.Select(aa => string.Join("-", aa))));
                    UnfrozenDownstreamReplyHandler(new Tuple<TupleMessage, string>(tupleMessage, Nonce));
                });
            });
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
            _frozenDownstreamReplies        = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenUpstreamReplies          = new ConcurrentBag<Process>();
            _frozenEpochChangeReplies       = new ConcurrentBag<Tuple<int, Process>>();
            _frozenPaxosReplies             = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenQuorumReplies            = new ConcurrentBag<Tuple<TupleMessage, string>>();

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
            Parallel.ForEach(_frozenInfrastructureRequests, frozenInfrastructureRequest => {
                _infrastructureRequestListener(frozenInfrastructureRequest);
            });
            Parallel.ForEach(_frozenDownstreamRequests, frozenDownstreamRequest => {
                _downstreamRequestListener(frozenDownstreamRequest);
            });
            Parallel.ForEach(_frozenUpstreamRequests, frozenUpstreamRequest => {
                _upstreamRequestListener(frozenUpstreamRequest);
            });

            //Send frozen replies
            Parallel.ForEach(_frozenInfrastructureReplies, frozenInfrastructureReply => {
                _infrastructureReplyListener(frozenInfrastructureReply);
            });
            Parallel.ForEach(_frozenDownstreamReplies, frozenDownstreamReply => {
                _downstreamReplyListener(frozenDownstreamReply);
            });
            Parallel.ForEach(_frozenUpstreamReplies, frozenUpstreamReply => {
                _upstreamReplyListener(frozenUpstreamReply);
            });
            Parallel.ForEach(_frozenEpochChangeReplies, frozenEpochChangeReply => {
                _epochChangeReplyListener(frozenEpochChangeReply.Item1, frozenEpochChangeReply.Item2);
            });
            Parallel.ForEach(_frozenPaxosReplies, frozenPaxosReply => {
                _paxosReplyListener(frozenPaxosReply);
            });
            Parallel.ForEach(_frozenQuorumReplies, frozenQuorumReply => {
                _quorumReplyListener(frozenQuorumReply);
            });

            //Reset frozen request sets
            _frozenInfrastructureRequests   = new ConcurrentBag<Message>();
            _frozenDownstreamRequests       = new ConcurrentBag<Message>();
            _frozenUpstreamRequests         = new ConcurrentBag<Message>();

            //Reset frozen reply sets
            _frozenInfrastructureReplies    = new ConcurrentBag<Message>();
            _frozenDownstreamReplies        = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenUpstreamReplies          = new ConcurrentBag<Process>();
            _frozenEpochChangeReplies       = new ConcurrentBag<Tuple<int, Process>>();
            _frozenPaxosReplies             = new ConcurrentBag<Tuple<TupleMessage, string>>();
            _frozenQuorumReplies            = new ConcurrentBag<Tuple<TupleMessage, string>>();
        }

        public void Status() {
            Console.WriteLine("Status:");
			Console.WriteLine("\t Operator type: \t" + _command.ToString());
            Console.WriteLine("\t Replication type: \t" + _serverType.ToString());
			Console.WriteLine("\t Routing Policy: \t" + _routingPolicy.ToString() + (_hashing > -1 && _routingPolicy == RoutingPolicy.HASHING ? "(" + _hashing + ")" : ""));
			Console.WriteLine("\t Semantics Policy: \t" + _semanticsPolicy.ToString());
			Console.WriteLine("\t Waiting interval: \t" + _sleepBetweenEvents);
			Console.WriteLine("\t State: \t\t" + _state);

            //TODO display more stuff in status?
            foreach (KeyValuePair<string, string> pair in _command.Status()) {
                Console.WriteLine("\t " + pair.Key + ": \t\t" + pair.Value);
            }

            Console.WriteLine("\t Receivers:");
            if (_downstreamBroadcast.Processes != null) {
                foreach (Process receiver in _downstreamBroadcast.Processes) {
                    Console.WriteLine("\t\t\t\t" + receiver.ServiceName + "  at " + receiver.Url);
                }
            }
        }

        public void Interval(int milliseconds) {
            _sleepBetweenEvents = milliseconds;
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
