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
    using TupleMessage = List<IList<String>>;

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
                                    _upstreamBroadcast,
                                    _downstreamBroadcast;
        private Action<Message> _infrastructureRequestListener,
                                _downstreamRequestListener,
                                _upstreamRequestListener;
        private Action<Tuple<RequestType, String>> _infrastructureReplyListener;
        private Action<TupleMessage> _downstreamReplyListener;
        private Action<Process> _upstreamReplyListener;

        //Consensus variables
        private int _timestamp;
        private IList<UniformConsensus<TupleMessage>> _paxusConsenti,
                                                      _quorumConsenti;
        #endregion

        #region Constructor
        private void DefineOperator(String operatorId, String url, String operatorSpec) {
			int fieldNumber = 0;
			Condition condition = Condition.UNDEFINED;
			String[] operatorSpecList = operatorSpec.Split(',');

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
        private void InitHandler(Tuple<RequestType, String> reply) {
            switch(reply.Item1) {
                case RequestType.PAXOS:
                    PaxosInitHandler(reply.Item2);
                    break;
                case RequestType.QUORUM:
                    QuorumInitHandler(reply.Item2);
                    break;
            }
        }

        private UniformConsensus<TupleMessage> PaxosInitHandler(String suffix) {
            Process[] suffixedReplications = _replications
                .Select((suffixedProcess) => suffixedProcess.Concat(suffix))
                .ToArray();

            UniformConsensus <TupleMessage> paxos =  new LeaderDrivenConsensus<TupleMessage>(
                _process.Concat(suffix),
                _replications.Count() + 1,
                PaxosReplyHandler,
                EpochChangeReplyHandler,
                false,
                suffixedReplications
            );
            _paxusConsenti.Add(paxos);

            return paxos;
        }

        private UniformConsensus<TupleMessage> QuorumInitHandler(String suffix) {
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

        private void QuorumRequestHandler(TupleMessage tupleMessage = null) {
            String suffix = _process.Url + "_" + _timestamp++;
            UniformConsensus<TupleMessage> quorum = QuorumInitHandler(suffix);
            InfrastructureRequestHandler(new Tuple<RequestType, String>(RequestType.QUORUM, suffix));
            Thread.Sleep(500);
            if(tupleMessage == null) {
                return;
            }
            quorum.Propose(tupleMessage);
        }

        private void PaxosRequestHandler(TupleMessage tupleMessage = null) {
            String suffix = _process.Url + "_" + _timestamp++;
            UniformConsensus<TupleMessage> paxos = PaxosInitHandler(suffix);
            InfrastructureRequestHandler(new Tuple<RequestType, String>(RequestType.PAXOS, suffix));
            Thread.Sleep(500);
            if (tupleMessage == null) {
                return;
            }
            paxos.Propose(tupleMessage);
        }
        #endregion
        #region Request Handlers
        private void InfrastructureRequestHandler(Tuple<RequestType, String> message) {
            _infrastructureRequestListener(message);
        }

        private void DownstreamRequestHandler(TupleMessage message) {
            _downstreamRequestListener(message);
        }

        private void UpstreamRequestHandler(Process message) {
            _upstreamRequestListener(message);
        }
        #endregion
        #region Reply Handlers
        private void InfrastructureReplyHandler(Process process, Message message) {
            _infrastructureReplyListener((Tuple<RequestType, String>)message);
        }

        private void DownstreamReplyHandler(Process process, Message message) {
            _downstreamReplyListener((TupleMessage)message);
        }

        private void UpstreamReplyHandler(Process process, Message message) {
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

        private void PaxosReplyHandler(TupleMessage tupleMessage) {
            Console.WriteLine("Executing " + String.Join(" , ", tupleMessage.Select(aa => String.Join("-", aa))));

            TupleMessage result = _command.Execute(tupleMessage);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            Thread.Sleep(_sleepBetweenEvents);

            if (result == null) {
                return;
            }

            foreach (List<String> tuple in result) {
                Log(LogStatus.FULL, String.Join(" - ", tuple));
            }

            QuorumRequestHandler(result);
        }

        private void QuorumReplyHandler(TupleMessage tupleMessage) {
            Console.WriteLine("Joining " + String.Join(" , ", tupleMessage.Select(aa => String.Join("-", aa))));
            if (_serverType == ServerType.PRIMARY) {
                Console.WriteLine("Sending " + String.Join(" , ", tupleMessage.Select(aa => String.Join("-", aa))));
                DownstreamRequestHandler(tupleMessage);
            }
        }
        #endregion
        #region Others
        private bool TryParseCondition(String value, out Condition condition) {
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

