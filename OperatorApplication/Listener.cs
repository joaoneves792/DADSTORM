using DistributedAlgoritmsClassLibrary;
using OperatorApplication.Commands;
using OperatorApplication.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OperatorApplication
{
    using Message = Object;
    using Timestamp = Int32;
    using TupleMessage = List<IList<string>>;

    internal partial class Operator {
        private enum ServerType {
            PRIMARY,
            REPLICATION
        }

        private enum RoutingPolicy {
            PRIMARY,
            RANDOM,
            HASHING
        }

        private enum SemanticsPolicy {
            AT_MOST_ONCE,
            AT_LEAST_ONCE,
            EXACTLY_ONCE
        }

        #region Variables
        //Operator variables
        private Process _process;
        private Process[] _replications;
        private Command _command;
        private ServerType _serverType;
        private RoutingPolicy _routingPolicy;
        private SemanticsPolicy _semanticsPolicy;
        private int _hashing;

        //Broadcast variables
        private BestEffortBroadcast _infrastructureBroadcast,
                                    _upstreamBroadcast;
        private MutableBroadcast _downstreamBroadcast;

        //Consensus variables
        private int _timestamp;
        private object _timestampLock;
        private IList<Tuple<TupleMessage, string>> _archives;
        private IDictionary<string, UniformConsensus<Tuple<TupleMessage, string>>> _paxosConsenti;
        private IDictionary<string, UniformConsensus<Tuple<TupleMessage, string>>> _quorumConsenti;

        //Request variables
        private Action<Message>                     _infrastructureRequestListener,
                                                    _downstreamRequestListener,
                                                    _upstreamRequestListener,

        //Reply variables
                                                    _infrastructureReplyListener;
        private Action<Tuple<TupleMessage, string>> _downstreamReplyListener;
        private Action<Process>                     _upstreamReplyListener;
        private Action<int, Process>                _epochChangeReplyListener;
        private Action<Tuple<TupleMessage, string>> _paxosReplyListener;
        private Action<Tuple<TupleMessage, string>> _quorumReplyListener;
        #endregion

        #region Constructor
        private void DefineOperator(string operatorId, string url, string operatorSpec) {
			int fieldNumber = 0;
			Condition condition = Condition.UNDEFINED;
			string[] operatorSpecList = operatorSpec.Split(',');

            _process = new Process(operatorId, url);
            Console.Title = _process.Uri;

            //Define operator command
            if (operatorSpecList[0].Equals("UNIQ") &&
				int.TryParse(operatorSpecList[1], out fieldNumber)) {
				_command = new UNIQCommand(fieldNumber);

			} else if (operatorSpecList[0].Equals("COUNT")) {
				_command = new COUNTCommand();

			} else if (operatorSpecList[0].Equals("DUP")) {
				_command = new DUPCommand();

			} else if (operatorSpecList[0].Equals("FILTER") &&
					   int.TryParse(operatorSpecList[1], out fieldNumber) &&
					   TryParseCondition(operatorSpecList[2], out condition)) {
				_command = new FILTERCommand(fieldNumber, condition, operatorSpecList[3]);

			} else if (operatorSpecList[0].Equals("CUSTOM")) {
				_command = new CUSTOMCommand(operatorSpecList[1], operatorSpecList[2], operatorSpecList[3]);

			} else {
				throw new InvalidOperatorException(operatorSpecList[0]);
			}
		}
        #endregion

        // Define Paxos and Quorum nonce
        private string Nonce {
            get {
                lock (_timestampLock) {
                    return _process.SuffixedUrl + "_" + _timestamp++;
                }
            }
        }

        #region Request Handlers
        private void InfrastructureRequestHandler(Message message) {
            _infrastructureRequestListener(message);
        }

        private void DownstreamRequestHandler(Tuple<TupleMessage, string> message) {
            _downstreamRequestListener(message);
        }

        private void UpstreamRequestHandler(Process message) {
            _upstreamRequestListener(message);
        }

        private void QuorumRequestHandler(Tuple<TupleMessage, string> request) {
            // Init Quorum
            UniformConsensus<Tuple<TupleMessage, string>> quorum = QuorumInitHandler(request.Item2);
            InfrastructureRequestHandler(request.Item2);

            // Propose Quorum value
            quorum.Propose(request);
        }
        #endregion
        #region Unfrozen Request Handlers
        private void UnfrozenInfrastructureRequestHandler(Message request) {
            _infrastructureBroadcast.Broadcast(request);
        }

        private void UnfrozenDownstreamRequestHandler(Message request) {
            _downstreamBroadcast.Broadcast(request);
        }

        private void UnfrozenUpstreamRequestHandler(Message request) {
            _upstreamBroadcast.Broadcast(request);
        }
        #endregion
        #region Reply Handlers
        private void InfrastructureReplyHandler(Process process, Message message) {
            if(process.Equals(_process)) {
                return;
            }
            _infrastructureReplyListener(message);
        }

        private void DownstreamReplyHandler(Process process, Message message) {
            if (process.Equals(_process)) {
                return;
            }
            _downstreamReplyListener((Tuple<TupleMessage, string>)message);
        }

        private void UpstreamReplyHandler(Process process, Message message) {
            if (process.Equals(_process)) {
                return;
            }
            _upstreamReplyListener((Process)message);
        }

        private void EpochChangeReplyHandler(Timestamp timestamp, Process process) {
            _epochChangeReplyListener(timestamp, process);
        }

        private void PaxosReplyHandler(Tuple<TupleMessage, string> reply) {
            _paxosReplyListener(reply);
        }

        private void QuorumReplyHandler(Tuple<TupleMessage, string> reply) {
            if (!reply.Item2.Contains(_process.SuffixedUrl)) {
                return;
            }
            _quorumReplyListener(reply);
        }
        #endregion
        #region Unfrozen Reply Handlers
        private void UnfrozenInfrastructureReplyHandler(Message reply) {
            if (reply is Tuple<string, Process>) {
                PaxosInitHandler((Tuple<string, Process>)reply);
            } else if (reply is string) {
                QuorumInitHandler((string)reply);
            }
        }

        private void UnfrozenDownstreamReplyHandler(Tuple<TupleMessage, string> reply) {
            if (_replications.Count() == 0) {
                UnfrozenPaxosReplyHandler(new Tuple<TupleMessage, string>(reply.Item1, null));
                return;
            }

            String suffix = reply.Item2 + "_" + _process.SuffixedUrl;
            UniformConsensus<Tuple<TupleMessage, string>> paxos;
            lock (_paxosConsenti) {
                paxos = _paxosConsenti
                    .Where(keyValuePair => {
                        //DEBUG: Console.WriteLine("Does " + keyValuePair.Key + " contain " + reply.Item2 + "?");
                        return keyValuePair.Key.Contains(reply.Item2);
                    })
                    .FirstOrDefault()
                    .Value;

                if (paxos == null) {
                    // Init Paxos
                    Tuple<string, Process> tuple = new Tuple<string, Process>(suffix, _process);
                    paxos = PaxosInitHandler(tuple);
                    InfrastructureRequestHandler(tuple);
                }
            }

            // Propose Paxos value
            paxos.Propose(new Tuple<TupleMessage, string>(reply.Item1, suffix));
        }

        private void UnfrozenUpstreamReplyHandler(Process reply) {
            _downstreamBroadcast.Connect(reply);

            //Console.WriteLine("New leader: " + reply.Url);

            // Send older but possibly unreached requests
            Parallel.ForEach(_archives, request => {
                //DEBUG: Console.WriteLine("Resending " + string.Join(" , ", request.Item1.Select(aa => string.Join("-", aa))));
                DownstreamRequestHandler(request);
            });
        }

        private void UnfrozenEpochChangeReplyHandler(Timestamp timestamp, Process process) {
            if (_process.Equals(process)) {
                PrimaryEpochChangeInitHandler();
            } else {
                ReplicationEpochChangeInitHandler();
            }
        }

        private void UnfrozenPaxosReplyHandler(Tuple<TupleMessage, string> reply) {
            //DEBUG: Console.WriteLine("Executing " + string.Join(" , ", reply.Item1.Select(aa => string.Join("-", aa))));

            TupleMessage result = _command.Execute(reply.Item1);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            Thread.Sleep(_sleepBetweenEvents);

            if (_command is COUNTCommand) {
                Log(LogStatus.FULL, _command.Status()[0].Value);
            }

            if (result == null) {
                return;
            }

            Parallel.ForEach(result, tuple => {
                Log(LogStatus.FULL, string.Join(" - ", tuple));
            });

            //Share result to downstream nodes if current node has no replications
            if (reply.Item2 == null) {
                UnfrozenQuorumReplyHandler(new Tuple<TupleMessage, string>(result, reply.Item2));
                return;
            }

            //Compare result with replications result
            QuorumRequestHandler(new Tuple<TupleMessage, string>(result, reply.Item2));
        }

        private void UnfrozenQuorumReplyHandler(Tuple<TupleMessage, string> reply) {
            //DEBUG: Console.WriteLine("Sending " + string.Join(" , ", reply.Item1.Select(aa => string.Join("-", aa))));

            Tuple<TupleMessage, string> request = new Tuple<TupleMessage, string>(reply.Item1, Nonce);

            // Save request for future epochs
            _archives.Add(request);

            DownstreamRequestHandler(request);
        }
        #endregion
        #region Init Handlers
        private void PrimaryEpochChangeInitHandler() {
            if(_serverType == ServerType.PRIMARY) {
                return;
            }
            _serverType = ServerType.PRIMARY;
            UpstreamRequestHandler(_process);
        }

        private void ReplicationEpochChangeInitHandler() {
            _serverType = ServerType.REPLICATION;
        }

        private UniformConsensus<Tuple<TupleMessage, string>> PaxosInitHandler(Tuple<string, Process> tuple) {
            Process[] suffixedReplications = _replications
                .Select((suffixedProcess) => suffixedProcess.Concat(tuple.Item1))
                .ToArray();

            UniformConsensus <Tuple<TupleMessage, string>> paxos =  new LeaderDrivenConsensus<Tuple<TupleMessage, string>>(
                _process.Concat(tuple.Item1),
                _replications.Count() + 1,
                PaxosReplyHandler,
                EpochChangeReplyHandler,
                tuple.Item2.Concat(tuple.Item1),
                suffixedReplications
            );
            _paxosConsenti.Add(tuple.Item1, paxos);

            return paxos;
        }
        private UniformConsensus<Tuple<TupleMessage, string>> QuorumInitHandler(string suffix) {
            Process[] suffixedReplications = _replications
                .Select((suffixedProcess) => suffixedProcess.Concat(suffix))
                .ToArray();

            UniformConsensus<Tuple<TupleMessage, string>> quorum = new FloodingUniformConsensus<Tuple<TupleMessage, string>>(
                _process.Concat(suffix),
                QuorumReplyHandler,
                suffixedReplications);
            _quorumConsenti.Add(suffix, quorum);

            return quorum;
        }
        #endregion
        #region Others
        private bool TryParseCondition(string value, out Condition condition) {
			condition = Condition.UNDEFINED;

			//Parse condition
			if (value.Equals(">")) {
				condition = Condition.GREATER_THAN;

			} else if (value.Equals("<")) {
				condition = Condition.LESS_THAN;

			} else if (value.Equals("=")) {
				condition = Condition.EQUALS;
			} else {
				return false;
			}

			return true;
		}
        #endregion
    }
}

