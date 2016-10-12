using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OperatorApplication.Commands;
using DistributedAlgoritmsClassLibrary;
using LoggingClassLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using TupleMessage = List<String>;
    using OperatorSpec = String;

    internal partial class Operator
    {
        private Action<Process, Message> _listener, _send;
        PointToPointLink _pointToPointLink;
        Process _process;

        Command _command;
        private IProducerConsumerCollection<Process> _outputReceivers;

        private void DefineOperatorType(OperatorSpec operatorSpec)
        {
            int fieldNumber = 0;
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
                     TryParseCondition(operatorSpecList[2], out condition))
            {
                Console.WriteLine("FILTER");
				_command = new FILTERCommand(fieldNumber, condition, operatorSpecList[3]);
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

        private void StoreMessage(Process process, Message message)
        {
            Tuple<Process, Message> request = new Tuple<Process, Message>(process, message);
            while (!_frozenRequests.TryAdd(request))
            {
                Log.WriteLine(LogStatus.DEBUG, "Message not froze");
            }
            Log.WriteLine(LogStatus.DEBUG, _frozenRequests.Count + " frozen requests");
        }

        private void StoreReply(Process process, Message message)
        {
            Tuple<Process, Message> reply = new Tuple<Process, Message>(process, message);
            while (!_frozenReplies.TryAdd(reply))
            {
                Log.WriteLine(LogStatus.DEBUG, "Message not froze");
            }
            Log.WriteLine(LogStatus.DEBUG, _frozenRequests.Count + " frozen replies");
        }

        private void ParseAndStoreMessage(Process process, Message message)
        {
            //Parse message
            if (message is TupleMessage)
            {
                Console.WriteLine("Received tuple " + String.Join(",", (TupleMessage)message) + " from process " + process.Name);
                StoreMessage(process, message);
            }
            else if (message is Process)
            {
                Console.WriteLine("Received url " + String.Join(",", (Process)message) + " from process " + process.Name);
                UrlMessageCommand((Process)message);
            }
        }

        private void ParseMessage(Process process, Message message)
        {
            //Parse message
            if (message is TupleMessage) {
                Console.WriteLine("Received tuple " + String.Join(",", (TupleMessage)message) + " from process " + process.Name);
                TupleMessageCommand((TupleMessage)message);
            } else if (message is Process) {
                Console.WriteLine("Received url " + String.Join(",", (Process)message) + " from process " + process.Name);
                UrlMessageCommand((Process)message);
            }
        }

        private void TupleMessageCommand(TupleMessage tupleMessage)
        {
            TupleMessage result = _command.Execute(tupleMessage);

            /*Sleep before passing on the results as ordered by the puppetMaster*/
            System.Threading.Thread.Sleep(_sleepBetweenEvents);

            if (result == null) {
                Console.WriteLine("Not sending tuple");
                return;
            }

            foreach (Process outputReceiver in _outputReceivers) {
                Console.WriteLine("Sending tuple " + String.Join(",", result) + " to process " + outputReceiver.Name);
                _send(outputReceiver, (Object)result);
            }
        }

        private void UrlMessageCommand(Process outputReceiver)
        {
            //Submit output receiver
            _pointToPointLink.Connect(outputReceiver);
            _outputReceivers.TryAdd(outputReceiver);

            Console.WriteLine("Added output receiver " + outputReceiver.Name);
        }
    }
}
