using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;

using DistributedAlgoritmsClassLibrary;

namespace OperatorApplication
{
    using Message = Object;
    using OperatorId = String;
    using Url = String;
    using InputOps = String;
    using Address = String;
    using OperatorSpec = String;
    using TupleMessage = List<String>;

    partial class Program
    {
        private static bool Matches(String pattern, String line, out GroupCollection groupCollection)
        {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            Match match = regex.Match(line);
            if (!match.Success)
            {
                groupCollection = null;
                return false;
            }

            groupCollection = match.Groups;
            return true;
        }

        static void Main(string[] args)
        {
            //Give names to arguments
            OperatorId      operatorId      = args[0];
            Url             url             = args[1];
            InputOps        inputOps        = args[2];
            Address         address         = args[3];
            OperatorSpec    operatorSpec    = args[4];

            //Define operator
            Process process     = new Process(operatorId, url);
            Listener listener   = new Listener(operatorSpec);

            //Register operator into remoting
            PointToPointLink pointToPointLink = new RemotingNode(process, listener.Deliver);

            //Get inputOps' sources
            String[] inputOpsList = inputOps.Split(',');
            GroupCollection groupCollection;
            FileStream fileStream;
            StreamReader streamReader;
            Process inputProcess;
            foreach (String inputOp in inputOpsList) {
                //Get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                    streamReader = new StreamReader(fileStream);
                    //TODO: parse file
                //Connect to process output
                } else if (Matches(URL, inputOp, out groupCollection)) {
                    inputProcess = new Process(inputOp, inputOp);

                    pointToPointLink.Connect(inputProcess);
                    pointToPointLink.Send(process, (Object) inputProcess);

                    listener.addInputSource(inputProcess);
                //Print error
                } else {
                    Console.WriteLine("Error: invalid input op");
                }
            }

            //TODO: MAYBE wait until next phase
		}

        private class Listener
        {
            Command command;
            private IDictionary<Process, bool> _inputSources;
            private IProducerConsumerCollection<Process> _outputReceivers;
            private IProducerConsumerCollection<String> _inputTuple;

            internal Listener(OperatorSpec operatorSpec) {
                _inputSources = new ConcurrentDictionary<Process, bool>();
                _outputReceivers = new ConcurrentBag<Process>();
                _inputTuple = new ConcurrentBag<String>();

                int fieldNumber = 0, value = 0;
                Condition condition = Condition.UNDEFINED;
                String[] operatorSpecList = operatorSpec.Split(',');

                //Define operator command
                if (operatorSpecList[0].Equals("UNIQ") &&
                    int.TryParse(operatorSpecList[1], out fieldNumber)) {
                    command = new UNIQCommand(fieldNumber);
                } else if (operatorSpecList[0].Equals("COUNT")) {
                    command = new COUNTCommand();
                } else if (operatorSpecList[0].Equals("DUP")) {
                    command = new DUPCommand();
                } else if (operatorSpecList[0].Equals("FILTER") &&
                           int.TryParse(operatorSpecList[1], out fieldNumber) &&
                           TryParseCondition(operatorSpecList[2], out condition) &&
                           int.TryParse(operatorSpecList[3], out value)) {
                    //FIXME: fix filter command constructor input
                    //command = new FILTERCommand(fieldNumber, condition, value);
                } else if (operatorSpecList[0].Equals("CUSTOM")) {
                    command = new CUSTOMCommand();
                } else {
                    Console.WriteLine("unrecognised.");
                }
            }

            internal void addInputSource(Process inputSource) {
                //Submit input source
                _inputSources.Add(inputSource, false);

                Console.WriteLine("Added input source " + inputSource.Name);
            }

            internal void addInputTuple(TupleMessage tupleMessage) {
                //Concatenate input tuples
                foreach(String message in tupleMessage) {
                    _inputTuple.TryAdd(message);
                    Console.WriteLine("Added tuple message " + message);
                }
            }

            private bool TryParseCondition(String value, out Condition condition)
            {
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

            internal void Deliver(Process process, Message message) {
                //Parse message
                if (message is TupleMessage) {
                    TupleMessageCommand(process, (TupleMessage) message);
                } else if (message is Process) {
                    UrlMessageCommand((Process) message);
                }
            }

            private void TupleMessageCommand(Process process, TupleMessage tupleMessage) {
                addInputTuple(tupleMessage);
                _inputSources[process] = true;

                bool obtainedAllInputs = _inputSources.Aggregate((a, b) => {
                    return new KeyValuePair<Process, bool>(null, a.Value & b.Value);
                }).Value;
                if (obtainedAllInputs) {
                    //FIXME: add tuple input
                    //command.Execute();
                }
            }

            private void UrlMessageCommand(Process outputReceiver) {
                //Submit output receiver
                _outputReceivers.TryAdd(outputReceiver);

                Console.WriteLine("Added output receiver " + outputReceiver.Name);
            }
        }
    }
}
