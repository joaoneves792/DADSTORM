using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Commands;
using DistributedAlgoritmsClassLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using TupleMessage = List<String>;
    using OperatorSpec = String;

    internal partial class Operator
    {
        private Action<Process, Message> _listener;
        PointToPointLink _pointToPointLink;
        Process _process;

        Command _command;
        private IProducerConsumerCollection<Process> _outputReceivers;

        private void DefineOperatorType(OperatorSpec operatorSpec)
        {
            int fieldNumber = 0, value = 0;
            Condition condition = Condition.UNDEFINED;
            String[] operatorSpecList = operatorSpec.Split(',');

            //Define operator command
            Console.WriteLine(operatorSpecList[0]);
            Console.WriteLine(operatorSpecList[0].Equals("UNIQ"));
            if(operatorSpecList[0].Equals("UNIQ"))
                int.TryParse(operatorSpecList[1], out fieldNumber);
            if (operatorSpecList[0].Equals("UNIQ") &&
                int.TryParse(operatorSpecList[1], out fieldNumber))
            {
                Console.WriteLine("UNIQ");
				//_command = new UNIQCommand(fieldNumber); // segundo o luis, field_number is the first element of the tuple
				_command = new UNIQCommand();
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
				//_command = new FILTERCommand(fieldNumber, condition, value); // same
				_command = new FILTERCommand(condition, value);
			} else if (operatorSpecList[0].Equals("CUSTOM"))
            {
                Console.WriteLine("CUSTOM");
                _command = new CUSTOMCommand();
            }
            else
            {
                Console.WriteLine("unrecognised.");
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
                TupleMessageCommand((TupleMessage)message);
            }
            else if (message is Process)
            {
                UrlMessageCommand((Process)message);
            }
        }

        private void TupleMessageCommand(TupleMessage tupleMessage)
        {
            //TODO: test me
            _waitHandle.WaitOne();

            TupleMessage result = _command.Execute(tupleMessage);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            System.Threading.Thread.Sleep(_sleepBetweenEvents);

            if (result == null) {
                return;
            }

            foreach (Process outputReceiver in _outputReceivers) {
                _pointToPointLink.Send(_process, (Object)result);
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
