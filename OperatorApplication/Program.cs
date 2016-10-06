using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

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

    internal partial class Operator {
        private readonly System.Threading.EventWaitHandle _waitHandle;

        internal Operator() {
            //Configuration component
            _waitHandle = new System.Threading.AutoResetEvent(false);

            //Execution component
            _process = null;
            _command = null;
            _pointToPointLink = null;
            _listener = ParseMessage;
            _outputReceivers = new ConcurrentBag<Process>();

            //Puppet component
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
            _process = new Process(operatorId, url);
            DefineOperatorType(operatorSpec);

            //Register operator into remoting
            _pointToPointLink = new RemotingNode(_process, Deliver);
            SubmitAsPuppet();

            //Get inputOps' sources
            String[] inputOpsList = inputOps.Split(',');
            GroupCollection groupCollection;
            FileStream fileStream;
            ICollection<StreamReader> inputFiles = new HashSet<StreamReader>();
            Process inputProcess;
            foreach (String inputOp in inputOpsList) {
                //Get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    Console.WriteLine("Identified path " + inputOp);

                    fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                    inputFiles.Add(new StreamReader(fileStream));
                } else if (Matches(URL, inputOp, out groupCollection)) {
                    Console.WriteLine("Identified url " + inputOp);

                    inputProcess = new Process(inputOp, inputOp);
                    _pointToPointLink.Connect(inputProcess);
                    _pointToPointLink.Send(_process, (Object)inputProcess);
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

            _waitHandle.WaitOne();

            //Process files
            foreach (StreamReader currentInputFile in inputFiles) {
                new Thread(() => {
                    StreamReader inputFile = currentInputFile;

                    String line;
                    while ((line = inputFile.ReadLine()) != null) {
                        new Thread(() => {
                            //Assumption: all files and lines are valid
                            TupleMessage tupleMessage = line.Split(',').ToList();
                            TupleMessageCommand(tupleMessage);
                        }).Start();
                    }
                    inputFile.Close();
                }).Start();
            }
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
