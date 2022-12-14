using CommonTypesLibrary;
using DistributedAlgoritmsClassLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;

namespace OperatorApplication
{
    using TupleMessage = List<IList<string>>;

    internal partial class Operator {
        #region Variables
        private ICollection<string> _inputFiles;
        #endregion

        #region Constructor
        internal Operator() {
            //Configuration component
            _inputFiles = new HashSet<string>();

            //Execution component
            _process = null;
            _replications = null;
            _command = null;
            _serverType = ServerType.UNDEFINED;
            _routingPolicy = RoutingPolicy.PRIMARY;
            _semanticsPolicy = SemanticsPolicy.AT_LEAST_ONCE;
            _hashing = -1;
            _infrastructureBroadcast = null;
            _upstreamBroadcast = null;
            _downstreamBroadcast = null;
            _timestamp = 0;
            _timestampLock = new object();
            _paxosConsenti = new Dictionary<string, UniformConsensus<Tuple<TupleMessage, string>>>();
            _quorumConsenti = new Dictionary<string, UniformConsensus<Tuple<TupleMessage, string>>>();

            //Puppet component
            _puppetMaster = null;
            _logStatus = LogStatus.LIGHT;
            _sleepBetweenEvents = 0;
            _state = null;
            Freeze();
            Flag.Frozen = false;
        }
        #endregion

        #region Configurator
        internal void Configure(string[] args) {
            //Give names to arguments
            string operatorId = args[0],
                   url = args[1],
                   inputOps = args[2],
                   replicas = args[3],
                   operatorSpec = args[4],
                   routing = args[5],
                   semantics = args[6],
                   loggingLevel = args[7];

            //Define operator
            DefineOperator(operatorId, url, operatorSpec);

            //Submit operator as nodes
            SubmitOperatorAsRemotingNode();
            SubmitOperatorAsPuppetryNode(loggingLevel);
            SubmitOperatorAsDownstreamNode(routing, semantics);
            SubmitOperatorAsUpstreamNode(inputOps);
            SubmitOperatorAsInfrastructureNode(operatorId, replicas);
            SubmitOperatorAsPrimaryNode();

            //Signals puppet master to continue commands execution
            _puppetMaster.ResetWaitHandle();
        }
        #endregion
        #region Submitters
        private void SubmitOperatorAsPrimaryNode() {
            if (_routingPolicy != RoutingPolicy.PRIMARY) {
                return;
            }

            if (_replications.Count() == 0) {
                PrimaryEpochChangeInitHandler();
                return;
            }

            try {
                IPuppet _puppet = (IPuppet)Activator.GetObject(
                    typeof(IPuppet),
                    _replications[0].Uri + "/Puppet");
                _puppet.ToString();

                ReplicationEpochChangeInitHandler();
            } catch (Exception) {
                PrimaryEpochChangeInitHandler();
            }
        }

        private void SubmitOperatorAsDownstreamNode(string routing, string semantics) {
            //Identify semantics policy
            PointToPointLink semanticsPolicy;
            if(semantics.Equals("at-most-once")) {
                _semanticsPolicy = SemanticsPolicy.AT_MOST_ONCE;
            } else if (semantics.Equals("at-least-once")) {
                _semanticsPolicy = SemanticsPolicy.AT_LEAST_ONCE;
            } else {
                _semanticsPolicy = SemanticsPolicy.EXACTLY_ONCE;
            }

            //Identify routing policy
            if (routing.Equals("primary")) {
                switch (_semanticsPolicy) {
                    case SemanticsPolicy.AT_MOST_ONCE:
                        semanticsPolicy = new RemotingNode(_process.Concat("ReliableBroadcast"), DownstreamReplyHandler);
                        break;
                    case SemanticsPolicy.AT_LEAST_ONCE:
                        semanticsPolicy = new RetransmitForever(_process.Concat("ReliableBroadcast"), DownstreamReplyHandler);
                        break;
                    case SemanticsPolicy.EXACTLY_ONCE:
                        semanticsPolicy = new EliminateDuplicates(_process.Concat("ReliableBroadcast"), DownstreamReplyHandler);
                        break;
                    default:
                        return;
                }
                _downstreamBroadcast = new PrimaryBroadcast(_process, semanticsPolicy);
                _routingPolicy = RoutingPolicy.PRIMARY;
            } else if (routing.Equals("random")) {
                switch (_semanticsPolicy) {
                    case SemanticsPolicy.AT_MOST_ONCE:
                        _downstreamBroadcast = new RandomBroadcast(_process, DownstreamReplyHandler);
                        break;
                    case SemanticsPolicy.AT_LEAST_ONCE:
                        _downstreamBroadcast = new StubbornRandomBroadcast(_process, DownstreamReplyHandler);
                        break;
                    case SemanticsPolicy.EXACTLY_ONCE:
                        _downstreamBroadcast = new PerfectRandomBroadcast(_process, DownstreamReplyHandler);
                        break;
                }
                _routingPolicy = RoutingPolicy.RANDOM;
            } else {
                GroupCollection groupCollection;
                Matches(HASHING, routing, out groupCollection);
                _hashing = Int32.Parse(groupCollection[1].Value);

                switch (_semanticsPolicy) {
                    case SemanticsPolicy.AT_MOST_ONCE:
                        _downstreamBroadcast = new HashingBroadcast(_process, DownstreamReplyHandler, _hashing);
                        break;
                    case SemanticsPolicy.AT_LEAST_ONCE:
                        _downstreamBroadcast = new StubbornHashingBroadcast(_process, DownstreamReplyHandler, _hashing);
                        break;
                    case SemanticsPolicy.EXACTLY_ONCE:
                        _downstreamBroadcast = new PerfectHashingBroadcast(_process, DownstreamReplyHandler, _hashing);
                        break;
                }
                _routingPolicy = RoutingPolicy.HASHING;
            }
        }

        private void SubmitOperatorAsUpstreamNode(string inputOps) {
            string[] inputOpsList = inputOps.Split(','), inputOpSource;
            GroupCollection groupCollection;
            Process process;
            IList<Process> processes = new List<Process>();

            foreach (string inputOp in inputOpsList) {
                //Or get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    _inputFiles.Add(inputOp);

                //Or get upstream Operator
                } else /*if (Matches(URL, inputOp, out groupCollection)) */{
                    inputOpSource = inputOp.Split('|');
                    process = new Process(inputOpSource[0], inputOpSource[1], new List<string> { "Upstream" });
                    processes.Add(process);
                }
            }

            //Define upstream broadcast network
            _upstreamBroadcast = new BasicBroadcast(_process.Concat("Upstream"), UpstreamReplyHandler, processes.ToArray());
            _upstreamRequestListener = UnfrozenUpstreamRequestHandler;
            _upstreamReplyListener = UnfrozenUpstreamReplyHandler;
            UpstreamRequestHandler(_process);
        }

        private void SubmitOperatorAsRemotingNode() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary remoteChannelProperties = new Hashtable();
            remoteChannelProperties["port"] = _process.Port;
            remoteChannelProperties["connectionTimeout"] = 100;
            TcpChannel channel = new TcpChannel(remoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);
        }

        private void SubmitOperatorAsInfrastructureNode(string operatorId, string replicas) {
            string[] replicaList = replicas.Split(',');
            GroupCollection groupCollection;
            Process process;
            IList<Process> processes = new List<Process>();

            foreach (string replica in replicaList) {
                if (Matches(URL, replica, out groupCollection)) {
                    process = new Process(operatorId, replica);
                    if (process.Equals(_process)) {
                        continue;
                    }
                    processes.Add(process);
                }
            }
            _replications = processes.ToArray();

            //Define infrastructure broadcast network
            Process[] suffixedReplications = _replications
                .Select((suffixedProcess) => suffixedProcess.Concat("Infrastructure"))
                .ToArray();
            _infrastructureBroadcast = new BasicBroadcast(_process.Concat("Infrastructure"), InfrastructureReplyHandler, suffixedReplications);
        }
        #endregion
        #region Others
        private bool Matches(string pattern, string line, out GroupCollection groupCollection) {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            Match match = regex.Match(line);
            if (!match.Success) {
                groupCollection = null;
                return false;
            }

            groupCollection = match.Groups;
            return true;
        }
        #endregion
    }

    public static class Program {
        public static void Main(string[] args) {
            Operator operatorWorker = new Operator();
            operatorWorker.Configure(args);
            Console.ReadLine();
        }
    }
}
