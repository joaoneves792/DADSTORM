using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OperatorApplication.Commands;
using DistributedAlgoritmsClassLibrary;
using OperatorApplication.Exceptions;

namespace OperatorApplication {
    using Message = Object;
    using TupleMessage = List<IList<String>>;
    using OperatorSpec = String;

    internal partial class Operator {

		private Action<Process, Message> _listener, _send;
		private PointToPointLink _pointToPointLink;
		private Process _process;

        private UniformConsensus<TupleMessage> _paxos;
        private IList<UniformConsensus<TupleMessage>> _quorumConsenti;
        private Process[] _replications;

		Command _command;
		private IProducerConsumerCollection<Process> _outputReceivers;

		private void DefineOperatorType(OperatorSpec operatorSpec) {
			int fieldNumber = 0;
			Condition condition = Condition.UNDEFINED;
			String[] operatorSpecList = operatorSpec.Split(',');

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

		private void Deliver(Process process, Message message) {
			_listener(process, message);
		}

		private void StoreMessage(Process process, Message message) {
			Tuple<Process, Message> request = new Tuple<Process, Message>(process, message);
			_frozenRequests.TryAdd(request);
		}

		private void StoreReply(Process process, Message message) {
			Tuple<Process, Message> reply = new Tuple<Process, Message>(process, message);
			_frozenReplies.TryAdd(reply);
		}

		private void ParseAndStoreMessage(Process process, Message message) {
			//Parse message
			if (message is TupleMessage) {
				StoreMessage(process, message);

			} else if (message is Process) {
				UrlMessageCommand((Process) message);
			}
		}

		private void ParseMessage(Process process, Message message) {
			//Parse message
			if (message is TupleMessage) {
                //TupleMessageCommand((TupleMessage) message);
                Console.WriteLine("ping 1");
                _paxos.Propose((TupleMessage)message);

			} else if (message is Process) {
				UrlMessageCommand((Process) message);
			}
		}

        private void PaxosDecided(TupleMessage value) {
            Console.WriteLine("ping 2");
            TupleMessageCommand(value);
        }

        private void TupleMessageCommand(TupleMessage tupleMessage) {
            TupleMessage result = _command.Execute(tupleMessage);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            System.Threading.Thread.Sleep(_sleepBetweenEvents);

            if (result == null) {
                return;
            }

            foreach (List<String> tuple in result)
            {
                Log(LogStatus.FULL, String.Join(" - ", tuple));
            }

            UniformConsensus<TupleMessage> quorumConsensus = new FloodingUniformConsensus<TupleMessage>(_process,
                                                                                                        QuorumConsensusDecided,
                                                                                                        _replications);

            Thread.Sleep(500);
            quorumConsensus.Propose(result);
            _quorumConsenti.Add(quorumConsensus);

			//foreach (Process outputReceiver in _outputReceivers) {
			//	_send(outputReceiver, (Object) result);
			//}
		}

		private void UrlMessageCommand(Process outputReceiver) {
			//Submit output receiver
			_pointToPointLink.Connect(outputReceiver);
			_outputReceivers.TryAdd(outputReceiver);
		}

        private void QuorumConsensusDecided(TupleMessage value) {
            foreach (Process outputReceiver in _outputReceivers) {
                _send(outputReceiver, (Object)value);
            }
        }
	}
}

