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

    internal partial class Operator {
        private bool _isReady;

        internal Operator() {
            //Configuration component
            _isReady = false;

            //Execution component
            _command = null;
            _listener = ParseMessage;
            _inputSources = new ConcurrentDictionary<Process, bool>();
            _outputReceivers = new ConcurrentBag<Process>();
            _inputTuple = new ConcurrentBag<String>();

            //Puppet component
            _frozenListener = ParseMessage;
            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
        }

        private bool Matches(String pattern, String line, out GroupCollection groupCollection) {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            Match match = regex.Match(line);
            if (!match.Success) {
                groupCollection = null;
                return false;
            }

            groupCollection = match.Groups;
            return true;
        }

        internal void Configure(string[] args) {
            //Give names to arguments
            OperatorId operatorId = args[0];
            Url url = args[1];
            InputOps inputOps = args[2];
            Address address = args[3];
            OperatorSpec operatorSpec = args[4];

            //Define operator
            Process process = new Process(operatorId, url);
            DefineOperatorType(operatorSpec);

            //Register operator into remoting
            PointToPointLink pointToPointLink = new RemotingNode(process, Deliver);
            SubmitAsPuppet(process);

            //Get inputOps' sources
            String[] inputOpsList = inputOps.Split(',');
            GroupCollection groupCollection;
            FileStream fileStream;
            StreamReader streamReader;
            Process inputProcess;
            foreach (String inputOp in inputOpsList) {
                //Get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    Console.WriteLine("Identified path " + inputOp);
                    fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                    streamReader = new StreamReader(fileStream);
                    //TODO: parse file
                    //Connect to process output
                } else if (Matches(URL, inputOp, out groupCollection)) {
                    Console.WriteLine("Identified url " + inputOp);
                    inputProcess = new Process(inputOp, inputOp);

                    pointToPointLink.Connect(inputProcess);
                    pointToPointLink.Send(process, (Object)inputProcess);

                    addInputSource(inputProcess);
                    //Print error
                }
                else
                {
                    Console.WriteLine("Error: invalid input op");
                    if (!Matches(PATH, inputOp, out groupCollection)) {
                        Console.WriteLine("Cause: unidentified path");
                    }
                    if (!File.Exists(inputOp))
                    {
                        Console.WriteLine("Cause: unidentified file " + Directory.GetCurrentDirectory());
                    }
                }
            }

            //TODO: MAYBE wait until next phase
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Operator operatorWorker = new Operator();
            operatorWorker.Configure(args);
            Console.ReadLine();
        }

        
    }
}
