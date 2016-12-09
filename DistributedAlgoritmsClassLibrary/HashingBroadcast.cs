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
        private PointToPointLink _pointToPointLink;
        private EventuallyPerfectFailureDetector _eventuallyPerfectFailureDetector;

        private IDictionary<string, IList<Process>> _correct;

        private IList<Message> _fromSelf;

        private readonly int _fieldId;

        public IList<Process> Processes {
            get {
                return _correct
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }


        public HashingBroadcast(Process process, PointToPointLink pointToPointLink, int fieldId) {
            _fieldId = fieldId;
            _correct = new Dictionary<string, IList<Process>>();
            _fromSelf = new List<Message>();
            _pointToPointLink = pointToPointLink;
            _eventuallyPerfectFailureDetector = new MaintainingTimeout(process.Concat(CLASSNAME), Suspect, Restore);
        }

        public void Suspect(Process process) {
            IDictionary<string, IList<Process>> correct;
            lock (_correct) {
                _correct[process.Name].Remove(process);
                correct = new Dictionary<string, IList<Process>>(_correct);
            }

            if (correct[process.Name].Count == 0) {
                return;
            }

            IList<Tuple<Process, Tuple<TupleMessage, string>>> requests = new List<Tuple<Process, Tuple<TupleMessage, string>>>();

            string nonce;
            Tuple<TupleMessage, string> request;
            Process nextProcess;
            foreach(Tuple<TupleMessage, string> message in _fromSelf) {
                nonce = message.Item2;

                Parallel.ForEach(message.Item1, tuple => {
                    request = new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce);
                    nextProcess = correct[process.Name][tuple[_fieldId].GetHashCode() % (correct[process.Name].Count)];

                    Console.WriteLine("Contingency to " + nextProcess.Url + " tuple " + String.Join("-", request.Item1[0]));
                    _pointToPointLink.Send(nextProcess, request);
                });
            }
        }

        public void Restore(Process process) {
            IDictionary<string, IList<Process>> correct;
            lock (_correct) {
                _correct[process.Name].Add(process);
                correct = new Dictionary<string, IList<Process>>(_correct);
            }

            if (correct[process.Name].Count != 1) {
                return;
            }

            IList<Tuple<Process, Tuple<TupleMessage, string>>> requests = new List<Tuple<Process, Tuple<TupleMessage, string>>>();

            string nonce;
            Tuple<TupleMessage, string> request;
            Process nextProcess;
            foreach(Tuple<TupleMessage, string> message in _fromSelf) {
                nonce = message.Item2;

                Parallel.ForEach(message.Item1, tuple => {
                    request = new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce);
                    nextProcess = correct[process.Name][tuple[_fieldId].GetHashCode() % (correct[process.Name].Count)];

                    Console.WriteLine("Contingency to " + nextProcess.Url + " tuple " + String.Join("-", request.Item1[0]));
                    _pointToPointLink.Send(nextProcess, request);
                });
            }
        }

        public void Broadcast(Message message) {
            // Save request for future epochs
            _fromSelf.Add(message);

            IDictionary<string, IList<Process>> correct;
            lock (_correct) {
                correct = new Dictionary<string, IList<Process>>(_correct);
            }

            Tuple<TupleMessage, string> messageTuple = (Tuple<TupleMessage, string>)message;
            IEnumerable<Process> processes;

            string nonce;
            Tuple<TupleMessage, string> request;

            nonce = messageTuple.Item2;
            foreach (IList<String> tuple in messageTuple.Item1) {
                request = new Tuple<TupleMessage, string>(new TupleMessage() { tuple }, nonce);
                processes = correct.Values.Select((list) => list[tuple[_fieldId].GetHashCode() % (list.Count)]);

                Parallel.ForEach(processes, process => {
                    _pointToPointLink.Send(process, request);
                });
            }
        }

        public void Connect(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            _eventuallyPerfectFailureDetector.Submit(suffixedProcess);

            lock (_correct) {
                //Create new list if list does not exist
                if (!_correct.ContainsKey(suffixedProcess.Name)) {
                    _correct.Add(suffixedProcess.Name, new List<Process>() { suffixedProcess });
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (list already exists)
                if (!_correct[suffixedProcess.Name].Contains(suffixedProcess)) {
                    _correct[suffixedProcess.Name].Add(suffixedProcess);
                    _pointToPointLink.Connect(suffixedProcess);
                }
            }
        }
    }
}
