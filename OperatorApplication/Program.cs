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

namespace OperatorApplication {
    using Message = Object;
    using OperatorId = String;
    using Url = String;
    using InputOps = String;
    using Replicas = String;
    using OperatorSpec = String;
    using TupleMessage = List<IList<String>>;

    internal partial class Operator {
        private readonly System.Threading.EventWaitHandle _waitHandle;

        internal Operator() {
            //Configuration component
            _waitHandle = new System.Threading.AutoResetEvent(false);

            //Execution component
            _process = null;
            _command = null;
            _pointToPointLink = null;
            _outputReceivers = new ConcurrentBag<Process>();
            _paxos = null;
            _quorumConsenti = new List<UniformConsensus<TupleMessage>>();

        //Puppet component
        _logStatus = LogStatus.LIGHT;
            _listener = ParseAndStoreMessage;
            _send = StoreReply;
            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenReplies = new ConcurrentBag<Tuple<Process, Message>>();

            _sleepBetweenEvents = 0;
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

        private void LoadStoredMessages() {
            _listener = ParseMessage;
            _send = _pointToPointLink.Send;

            foreach (Tuple<Process, Message> request in _frozenRequests) {
                new Thread(() => {
                    _listener(request.Item1, request.Item2);
                }).Start();
            }
            foreach (Tuple<Process, Message> reply in _frozenReplies) {
                new Thread(() => {
                    _send(reply.Item1, reply.Item2);
                }).Start();
            }

            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenReplies = new ConcurrentBag<Tuple<Process, Message>>();
        }

        internal void Configure(string[] args) {
            //Give names to arguments
            OperatorId operatorId = args[0];
            Url url = args[1];
            InputOps inputOps = args[2];
            Replicas replicas = args[3];
            OperatorSpec operatorSpec = args[4];

            //Define operator
            _process = new Process(operatorId, url);
            DefineOperatorType(operatorSpec);

            //Define redundancy infrastructure
            String[] replicaList = replicas.Split(',');
            GroupCollection groupCollection;
            Process replicaProcess;
            IList<Process> replicaProcessList = new List<Process>();
            foreach (String replica in replicaList) {
                if (Matches(URL, replica, out groupCollection)) {
                    replicaProcess = new Process(operatorId, replica);
                    if(replicaProcess.Equals(_process)) {
                        continue;
                    }
                    replicaProcessList.Add(replicaProcess);
                }
            }
            _replications = replicaProcessList.ToArray();
            Console.WriteLine("Replicas:\n" + String.Join("\n ", replicaProcessList));
            //Console.ReadLine();
            _paxos = new LeaderDrivenConsensus<TupleMessage>(_process,
                                                             _replications.Count() + 1,
                                                             PaxosDecided,
                                                             _replications);
            Console.WriteLine("Not passing");

            //Register operator into remoting
            _pointToPointLink = new RemotingNode(_process, Deliver);
            _send = _pointToPointLink.Send;
            SubmitAsPuppet();

            //Get inputOps' sources
            String[] inputOpsList = inputOps.Split(',');
            //GroupCollection groupCollection;
            FileStream fileStream;
            ICollection<StreamReader> inputFiles = new HashSet<StreamReader>();
            Process inputProcess;
            foreach (String inputOp in inputOpsList) {
                //Or get file data
                if (Matches(PATH, inputOp, out groupCollection) && File.Exists(inputOp)) {
                    fileStream = File.Open(inputOp, FileMode.Open, FileAccess.Read, FileShare.Read);
                    inputFiles.Add(new StreamReader(fileStream));
                //Or get upstream Operator
                } else if (Matches(URL, inputOp, out groupCollection)) {
                    inputProcess = new Process(inputOp, inputOp);
                    _pointToPointLink.Connect(inputProcess);
                    _pointToPointLink.Send(inputProcess, (Object)_process);
                }
                else
                {
                    //throw exception
                    //Console.WriteLine("Error: invalid input op");
                    if (!Matches(PATH, inputOp, out groupCollection)) {
                        //throw exception
                        //Console.WriteLine("Cause: unidentified path");
                    }
                    if (!File.Exists(inputOp))
                    {
                        //throw exception
                        //Console.WriteLine("Cause: unidentified file " + Directory.GetCurrentDirectory());
                    }
                }
            }

            _waitHandle.WaitOne();

            //Process already received tuples
            LoadStoredMessages();

            //Process files
            foreach (StreamReader currentInputFile in inputFiles) {
                ThreadPool.QueueUserWorkItem((inputFileObject) => {
                    StreamReader inputFile = (StreamReader)inputFileObject;

                    String currentLine;
                    while ((currentLine = inputFile.ReadLine()) != null) {
                        new Thread((lineObject) => {
                            //Assumption: all files and lines are valid
                            String line = (String)lineObject;
                            TupleMessage tupleMessage = new TupleMessage();
                            tupleMessage.Add(line.Split(',').ToList());
                            TupleMessageCommand(tupleMessage);
                        }).Start((Object)currentLine);
                    }
                    inputFile.Close();
                },(Object)currentInputFile);
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
