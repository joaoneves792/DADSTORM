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
    using TupleMessage = List<IList<String>>;

    internal partial class Operator {
        private enum RequestType {
            PAXOS,
            QUORUM
        }

        #region Variables
        private ICollection<StreamReader> _inputFiles;
        #endregion

        #region Constructor
        internal Operator() {
            //Configuration component
            _inputFiles = new HashSet<StreamReader>();

            //Execution component
            _process = null;
            _replications = null;
            _command = null;
            _serverType = ServerType.UNDEFINED;
            _infrastructureBroadcast = null;
            _upstreamBroadcast = null;
            _downstreamBroadcast = null;
            _timestamp = 0;
            _paxusConsenti = new List<UniformConsensus<TupleMessage>>();
            _quorumConsenti = new List<UniformConsensus<TupleMessage>>();

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
            String operatorId = args[0],
                   url = args[1],
                   inputOps = args[2],
                   replicas = args[3],
                   operatorSpec = args[4];

            //Define operator
            DefineOperator(operatorId, url, operatorSpec);

            //Submit operator as nodes
            SubmitOperatorAsRemotingNode();
            SubmitOperatorAsPuppetryNode();
            SubmitOperatorAsDownstreamNode();
            SubmitOperatorAsUpstreamNode(inputOps);
            SubmitOperatorAsInfrastructureNode(operatorId, replicas);
            SubmitOperatorAsPrimaryNode();

            //Signals puppet master to continue commands execution
            _puppetMaster.ResetWaitHandle();
        }
        #endregion
        #region Submitters
        private void SubmitOperatorAsPrimaryNode() {
            if(_replications.Count() == 0) {
                PrimaryEpochChangeReplyHandler();
                return;
            }

            try {
                IPuppet _puppet = (IPuppet)Activator.GetObject(
                    typeof(IPuppet),
                    _replications[0].Uri + "/Puppet");
                _puppet.ToString();

                ReplicationEpochChangeReplyHandler();
            } catch (Exception) {
                PrimaryEpochChangeReplyHandler();
            }
        }

        private void SubmitOperatorAsDownstreamNode() {
            //Define downstream broadcast network
            _downstreamBroadcast = new BasicBroadcast(_process.Concat("Downstream"), DownstreamReplyHandler);
            _downstreamRequestListener = _downstreamBroadcast.Broadcast;
            _upstreamReplyListener = _downstreamBroadcast.Connect;
        }

        private void SubmitOperatorAsUpstreamNode(string inputOps) {
            String[] inputOpsList = inputOps.Split(','), inputOpSource;
            GroupCollection groupCollection;
            Process process;
            IList<Process> processes = new List<Process>();
            FileStream fileStream;

            foreach (String inputOp in inputOpsList) {
                //Or get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _inputFiles.Add(new StreamReader(fileStream));

                //Or get upstream Operator
                } else /*if (Matches(URL, inputOp, out groupCollection)) */{
                    inputOpSource = inputOp.Split('|');
                    process = new Process(inputOpSource[0], inputOpSource[1], new List<String> { "Upstream" });
                    processes.Add(process);
                }
            }

            //Define upstream broadcast network
            _upstreamBroadcast = new BasicBroadcast(_process.Concat("Upstream"), UpstreamReplyHandler, processes.ToArray());
            _upstreamRequestListener = _upstreamBroadcast.Broadcast;
        }

        private void SubmitOperatorAsRemotingNode() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = _process.Port;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);
        }

        private void SubmitOperatorAsInfrastructureNode(string operatorId, string replicas) {
            String[] replicaList = replicas.Split(',');
            GroupCollection groupCollection;
            Process process;
            IList<Process> processes = new List<Process>();

            foreach (String replica in replicaList) {
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
        private bool Matches(String pattern, String line, out GroupCollection groupCollection) {
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
