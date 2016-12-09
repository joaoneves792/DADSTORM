using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using TupleMessage = List<IList<string>>;

    public class HashingBroadcast : ReliableBroadcast {
        private const string CLASSNAME = "ReliableBroadcast";
        private FairLossPointToPointLink _fairLossPointToPointLink;

        private IDictionary<string, IList<Process>> _correct;

        private readonly int _fieldId;

        public IList<Process> Processes {
            get {
                return _correct
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }


        public HashingBroadcast(Process process, Action<Process, Message> listener, int fieldId) {
            _fieldId = fieldId;
            _correct = new Dictionary<string, IList<Process>>();
            _fairLossPointToPointLink = new RemotingNode(process.Concat(CLASSNAME), listener);
        }

        public void Broadcast(Message message) {
            Console.WriteLine("Breastcast");
            IDictionary<string, IList<Process>> correct;
            lock (_correct) {
                correct = new Dictionary<string, IList<Process>>(_correct);
            }

            Tuple<TupleMessage, string> messageTuple;
            if (message is Tuple<string, Message>) {
                messageTuple = (Tuple<TupleMessage, string>)((Tuple<string, Message>) message).Item2;
            } else {
                messageTuple = (Tuple<TupleMessage, string>)message;
            }

            IEnumerable<Process> processes;

            string nonce;
            object request;

            nonce = messageTuple.Item2;
            foreach (IList<String> tuple in messageTuple.Item1) {
                if (message is Tuple<string, Message>) {
                    request = new Tuple<string, Message>(
                        ((Tuple<string, Message>)message).Item1,
                        new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce)
                    );
                } else {
                    request = new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce);
                }
                processes = correct.Values.Select((list) => list[(tuple[_fieldId].GetHashCode()) % (list.Count)]);

                Parallel.ForEach(processes, process => {
                    _fairLossPointToPointLink.Send(process, request);
                });
            }
        }

        public void Broadcast(Message message, int rotation) {
            IDictionary<string, IList<Process>> correct;
            lock (_correct) {
                correct = new Dictionary<string, IList<Process>>(_correct);
            }

            Tuple<TupleMessage, string> messageTuple;
            if (message is Tuple<string, Message>) {
                messageTuple = (Tuple<TupleMessage, string>)((Tuple<string, Message>) message).Item2;
            } else {
                messageTuple = (Tuple<TupleMessage, string>)message;
            }

            IEnumerable<Process> processes;

            string nonce;
            object request;

            nonce = messageTuple.Item2;
            foreach (IList<String> tuple in messageTuple.Item1) {
                if (message is Tuple<string, Message>) {
                    request = new Tuple<string, Message>(
                        ((Tuple<string, Message>)message).Item1,
                        new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce)
                    );
                } else {
                    request = new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce);
                }
                processes = correct.Values.Select((list) => list[(tuple[_fieldId].GetHashCode() + rotation) % (list.Count)]);

                Parallel.ForEach(processes, process => {
                    _fairLossPointToPointLink.Send(process, request);
                });
            }
        }

        public void Connect(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            lock (_correct) {
                //Create new list if list does not exist
                if (!_correct.ContainsKey(suffixedProcess.Name)) {
                    _correct.Add(suffixedProcess.Name, new List<Process>() { suffixedProcess });
                    _fairLossPointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (list already exists)
                if (!_correct[suffixedProcess.Name].Contains(suffixedProcess)) {
                    _correct[suffixedProcess.Name].Add(suffixedProcess);
                    _fairLossPointToPointLink.Connect(suffixedProcess);
                }
            }
        }
    }
}
