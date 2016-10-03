using System;
using System.Collections.Generic;
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
                    //TODO: send url message
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

            internal Listener(OperatorSpec operatorSpec) {
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

            internal void Deliver(Process process, Message message)
            {
                //Parse message
                if (message is TupleMessage) {
                    TupleMessageCommand((TupleMessage) message);
                } else if (message is UrlMessage) {
                    UrlMessageCommand((UrlMessage) message);
                }
            }

            private void TupleMessageCommand(TupleMessage tupleMessage) {
                //TODO: Implement me

                //receive input
                //verify all inputs
                //return if not verified
                //process tuples
            }

            private void UrlMessageCommand(UrlMessage urlMessage) {
                //TODO: Implement me
            }
        }
    }
}
