using DistributedAlgoritmsClassLibrary;
using OperatorApplication.Commands;
using OperatorApplication.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        private Action<Message> _infrastructureRequestListener,
                                _downstreamRequestListener,
                                _upstreamRequestListener,
                                _infrastructureReplyListener;
        private Action<TupleMessage> _downstreamReplyListener;
        private Action<Process> _upstreamReplyListener;

        //Consensus variables
        private int _timestamp;
        private IList<UniformConsensus<Tuple<TupleMessage, string>>> _paxusConsenti;
        private IList<UniformConsensus<TupleMessage>> _quorumConsenti;
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

        #region Init Handlers
        private void InitHandler(Message reply) {
            if (reply is Tuple<string, Process>) {
                PaxosInitHandler((Tuple<string, Process>)reply);
            } else if (reply is string) {
                QuorumInitHandler((string)reply);
            }
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
            UniformConsensus<TupleMessage> quorum = QuorumInitHandler(suffix);
            InfrastructureRequestHandler(suffix);
            Thread.Sleep(500);
            if(tupleMessage == null) {
                return;
            }
            quorum.Propose(tupleMessage);
        }

        private void PaxosRequestHandler(TupleMessage tupleMessage) {
            String suffix = _process.Url + "_" + _timestamp++;
            Tuple<string, Process> tuple = new Tuple<string, Process>(suffix, _process.Concat(suffix));
            UniformConsensus<Tuple<TupleMessage, string>> paxos = PaxosInitHandler(tuple);
            InfrastructureRequestHandler(tuple);
            Thread.Sleep(500);
            if (tupleMessage == null) {
                return;
            }
            paxos.Propose(new Tuple<TupleMessage, string>(tupleMessage, suffix));
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
            if (_process.Equals(process)) {
                PrimaryEpochChangeReplyHandler();
            } else {
                ReplicationEpochChangeReplyHandler();
            }
        }

        private void PrimaryEpochChangeReplyHandler() {
            if(_serverType == ServerType.PRIMARY) {
                return;
            }
            _serverType = ServerType.PRIMARY;
            UpstreamRequestHandler(_process);
        }

        private void ReplicationEpochChangeReplyHandler() {
            if (_serverType == ServerType.REPLICATION) {
                return;
            }
            _serverType = ServerType.REPLICATION;
        }

        private void PaxosReplyHandler(Tuple<TupleMessage, string> value) {
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

            //Share result to downstream nodes if current node has no replications
            if (_replications.Count() == 0) {
                QuorumReplyHandler(result);
                return;
            }

            //Compare result with replications result
            QuorumRequestHandler(result, value.Item2);
        }

        private void QuorumReplyHandler(TupleMessage tupleMessage) {
            Console.WriteLine("Joining " + string.Join(" , ", tupleMessage.Select(aa => string.Join("-", aa))));
            if (_serverType == ServerType.PRIMARY) {
                Console.WriteLine("Sending " + string.Join(" , ", tupleMessage.Select(aa => string.Join("-", aa))));
                DownstreamRequestHandler(tupleMessage);
            }
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

