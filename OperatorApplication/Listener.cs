using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DistributedAlgoritmsClassLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using TupleMessage = List<String>;
    using OperatorSpec = String;

    internal partial class Operator
    {
        private Action<Process, Message> _listener;

        Command _command;
        private IDictionary<Process, bool> _inputSources;
        private IProducerConsumerCollection<Process> _outputReceivers;
        private IProducerConsumerCollection<String> _inputTuple;

        private void DefineOperatorType(OperatorSpec operatorSpec)
        {
            int fieldNumber = 0, value = 0;
            Condition condition = Condition.UNDEFINED;
            String[] operatorSpecList = operatorSpec.Split(',');

            //Define operator command
            if (operatorSpecList[0].Equals("UNIQ") &&
                int.TryParse(operatorSpecList[1], out fieldNumber))
            {
                Console.WriteLine("UNIQ");
                _command = new UNIQCommand(fieldNumber);
            }
            else if (operatorSpecList[0].Equals("COUNT"))
            {
                Console.WriteLine("COUNT");
                _command = new COUNTCommand();
            }
            else if (operatorSpecList[0].Equals("DUP"))
            {
                Console.WriteLine("DUP");
                _command = new DUPCommand();
            }
            else if (operatorSpecList[0].Equals("FILTER") &&
                     int.TryParse(operatorSpecList[1], out fieldNumber) &&
                     TryParseCondition(operatorSpecList[2], out condition) &&
                     int.TryParse(operatorSpecList[3], out value))
            {
                Console.WriteLine("FILTER");
				//DONE :: FIXME: fix filter command constructor input
				_command = new FILTERCommand(fieldNumber, condition, value);
			}
            else if (operatorSpecList[0].Equals("CUSTOM"))
            {
                Console.WriteLine("CUSTOM");
                _command = new CUSTOMCommand();
            }
            else
            {
                Console.WriteLine("unrecognised.");
            }
        }

        private void addInputSource(Process inputSource)
        {
            //Submit input source
            _inputSources.Add(inputSource, false);

            Console.WriteLine("Added input source " + inputSource.Name);
        }

        private void addInputTuple(TupleMessage tupleMessage)
        {
            //Concatenate input tuples
            foreach (String message in tupleMessage)
            {
                _inputTuple.TryAdd(message);
                Console.WriteLine("Added tuple message " + message);
            }
        }

        private bool TryParseCondition(String value, out Condition condition)
        {
            condition = Condition.UNDEFINED;

            //Parse condition
            if (value.Equals(">"))
            {
                condition = Condition.GREATER_THAN;
            }
            else if (value.Equals("<"))
            {
                condition = Condition.LESS_THAN;
            }
            else if (value.Equals("="))
            {
                condition = Condition.EQUALS;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void Deliver(Process process, Message message)
        {
            _listener(process, message);
        }

        private void ParseMessage(Process process, Message message)
        {
            //Parse message
            if (message is TupleMessage)
            {
                TupleMessageCommand(process, (TupleMessage)message);
            }
            else if (message is Process)
            {
                UrlMessageCommand((Process)message);
            }
        }

        private void TupleMessageCommand(Process process, TupleMessage tupleMessage)
        {
            addInputTuple(tupleMessage);
            _inputSources[process] = true;

            TryExecuteCommand();
        }

        private void TryExecuteCommand() {
            bool obtainedAllInputs = _inputSources.Aggregate((a, b) => {
                return new KeyValuePair<Process, bool>(null, a.Value & b.Value);
            }).Value;
            if (obtainedAllInputs) {
                //FIXME: add tuple input
                //command.Execute();
            }
        }

        private void UrlMessageCommand(Process outputReceiver)
        {
            //Submit output receiver
            _outputReceivers.TryAdd(outputReceiver);

            Console.WriteLine("Added output receiver " + outputReceiver.Name);
        }
    }
}
