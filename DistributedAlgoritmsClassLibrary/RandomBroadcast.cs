using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RandomBroadcast : ReliableBroadcast {
        private const string CLASSNAME = "ReliableBroadcast";
        private PointToPointLink _pointToPointLink;
        private EventuallyPerfectFailureDetector _eventuallyPerfectFailureDetector;

        private IDictionary<string, IList<Process>> _correct;

        private IList<Message> _fromSelf;

        public IList<Process> Processes {
            get {
                return _correct
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }

        private IEnumerable<Process> Correct {
            get {
                return _correct
                    .Values
                    .Select((list) => list[new Random().Next(list.Count)]);
            }
        }

        public RandomBroadcast(Process process, PointToPointLink pointToPointLink) {
            _correct = new Dictionary<string, IList<Process>>();
            _fromSelf = new List<Message>();
            _pointToPointLink = pointToPointLink;
            _eventuallyPerfectFailureDetector = new MaintainingTimeout(process.Concat(CLASSNAME), Suspect, Restore);
        }

        public void Suspect(Process process) {
            Process nextProcess;

            lock (_correct) {
                _correct[process.Name].Remove(process);

                if (_correct[process.Name].Count == 0) {
                    return;
                }

                nextProcess = _correct[process.Name][new Random().Next(_correct[process.Name].Count)];
            }

            Parallel.ForEach(_fromSelf, message => {
                _pointToPointLink.Send(nextProcess, message);
            });
        }

        public void Restore(Process process) {
            Process nextProcess;

            lock (_correct) {
                _correct[process.Name].Add(process);

                if (_correct[process.Name].Count != 1) {
                    return;
                }

                nextProcess = _correct[process.Name][0];
            }

            Parallel.ForEach(_fromSelf, message => {
                _pointToPointLink.Send(nextProcess, message);
            });
        }

        public void Broadcast(Message message) {
            // Save request for future epochs
            _fromSelf.Add(message);

            Parallel.ForEach(Correct, process => {
                _pointToPointLink.Send(process, message);
            });
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
