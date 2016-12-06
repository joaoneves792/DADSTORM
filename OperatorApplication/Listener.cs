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
            UNDEFINED,
            PRIMARY,
            REPLICATION
        }

        #region Variables
        //Operator variables
        private Process _process;
        private Process[] _replications;
        private Command _command;
        private ServerType _serverType;

        //Broadcast variables
        private BestEffortBroadcast _infrastructureBroadcast,
                                    _upstreamBroadcast;
        private MutableBroadcast _downstreamBroadcast;

        //Consensus variables
        private int _timestamp;
        private object _timestampLock;
        private IList<UniformConsensus<Tuple<TupleMessage, string>>> _paxusConsenti;
        private IList<UniformConsensus<TupleMessage>> _quorumConsenti;

        //Request variables
        private Action<Message>                     _infrastructureRequestListener,
                                                    _downstreamRequestListener,
                                                    _upstreamRequestListener,

        //Reply variables
                                                    _infrastructureReplyListener;
        private Action<TupleMessage>                _downstreamReplyListener;
        private Action<Process>                     _upstreamReplyListener;
        private Action<int, Process>                _epochChangeReplyListener;
        private Action<Tuple<TupleMessage, string>> _paxosReplyListener;
        private Action<TupleMessage>                _quorumReplyListener;
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


        #region Request Handlers
        private void InfrastructureRequestHandler(Message message) {
            _infrastructureRequestListener(message);
        }

        private void DownstreamRequestHandler(TupleMessage message) {
            _downstreamRequestListener(message);
        }

        private void UpstreamRequestHandler(Process message) {
            _upstreamRequestListener(message);
        }

        private void QuorumRequestHandler(TupleMessage tupleMessage, String suffix) {
            // Init Quorum
            UniformConsensus<TupleMessage> quorum = QuorumInitHandler(suffix);
            InfrastructureRequestHandler(suffix);

            // Propose Quorum value
            quorum.Propose(tupleMessage);
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
            _downstreamReplyListener((TupleMessage)message);
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

        private void PaxosReplyHandler(Tuple<TupleMessage, string> value) {
            _paxosReplyListener(value);
        }

        private void QuorumReplyHandler(TupleMessage tupleMessage) {
            if (_serverType == ServerType.REPLICATION) {
                return;
            }
            _quorumReplyListener(tupleMessage);
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

        private void UnfrozenDownstreamReplyHandler(TupleMessage tupleMessage) {
            lock (_timestampLock) {
                // Define Paxos and Quorum nonce
                String suffix = _process.Url + "_" + _timestamp++;
                Tuple<string, Process> tuple = new Tuple<string, Process>(suffix, _process.Concat(suffix));

                // Init Paxos
                UniformConsensus<Tuple<TupleMessage, string>> paxos = PaxosInitHandler(tuple);
                Task.Run(() => { InfrastructureRequestHandler(tuple); });

                // Propose Paxos value
                Task.Run(() => { paxos.Propose(new Tuple<TupleMessage, string>(tupleMessage, suffix)); });
            }
        }

        private void UnfrozenUpstreamReplyHandler(Process reply) {
            _downstreamBroadcast.Connect(reply);
        }

        private void UnfrozenEpochChangeReplyHandler(Timestamp timestamp, Process process) {
            if (_process.Equals(process)) {
                PrimaryEpochChangeInitHandler();
            } else {
                ReplicationEpochChangeInitHandler();
            }
        }

        private void UnfrozenPaxosReplyHandler(Tuple<TupleMessage, string> value) {
            Console.WriteLine("Executing " + string.Join(" , ", value.Item1.Select(aa => string.Join("-", aa))));

            TupleMessage result = _command.Execute(value.Item1);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            Thread.Sleep(_sleepBetweenEvents);

            if (result == null) {
                return;
            }

            foreach (List<string> tuple in result) {
                Log(LogStatus.FULL, string.Join(" - ", tuple));
            }

            Console.WriteLine("#Replications: " + _replications.Count());

            //Share result to downstream nodes if current node has no replications
            if (_replications.Count() == 0) {
                QuorumReplyHandler(result);
                return;
            }

            //Compare result with replications result
            QuorumRequestHandler(result, value.Item2);
        }

        private void UnfrozenQuorumReplyHandler(TupleMessage tupleMessage) {
            Console.WriteLine("Sending " + string.Join(" , ", tupleMessage.Select(aa => string.Join("-", aa))));
            DownstreamRequestHandler(tupleMessage);
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
            if (_serverType == ServerType.REPLICATION) {
                return;
            }
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
                tuple.Item2,
                suffixedReplications
            );
            _paxusConsenti.Add(paxos);

            return paxos;
        }
        private UniformConsensus<TupleMessage> QuorumInitHandler(string suffix) {
            Process[] suffixedReplications = _replications
                .Select((suffixedProcess) => suffixedProcess.Concat(suffix))
                .ToArray();

            UniformConsensus<TupleMessage> quorum = new FloodingUniformConsensus<TupleMessage>(
                _process.Concat(suffix),
                QuorumReplyHandler,
                suffixedReplications);
            _quorumConsenti.Add(quorum);

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

