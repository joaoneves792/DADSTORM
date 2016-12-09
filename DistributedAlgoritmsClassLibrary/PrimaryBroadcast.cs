using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class PrimaryBroadcast : ReliableBroadcast {
        private const string CLASSNAME = "ReliableBroadcast";
        private PointToPointLink _pointToPointLink;
        private EventuallyPerfectFailureDetector _eventuallyPerfectFailureDetector;

        private IDictionary<string, Tuple<Process, IList<Process>>> _correct;

        private IList<Message> _fromSelf;

        public IList<Process> Processes {
            get {
                return _correct
                    .Values
                    .Select((tuple) => tuple.Item2)
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }

        private IEnumerable<Process> Correct {
            get {
                return _correct
                    .Values
                    .Select((tuple) => tuple.Item1);
            }
        }

        public PrimaryBroadcast(Process process, PointToPointLink pointToPointLink) {
            _correct = new Dictionary<string, Tuple<Process, IList<Process>>>();
            _fromSelf = new List<Message>();
            _pointToPointLink = pointToPointLink;
            _eventuallyPerfectFailureDetector = new MaintainingTimeout(process.Concat(CLASSNAME), Suspect, Restore);
        }

        public void Suspect(Process process) {
        }

        public void Restore(Process process) {
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
                //Create new tuple if tuple does not exist
                if (!_correct.ContainsKey(suffixedProcess.Name)) {
                    _correct.Add(suffixedProcess.Name, new Tuple<Process, IList<Process>>(suffixedProcess, new List<Process>(){ suffixedProcess }));
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (tuple already exists)
                if (!_correct[suffixedProcess.Name].Item2.Contains(suffixedProcess)) {
                    _correct[suffixedProcess.Name].Item2.Add(suffixedProcess);
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Change primary if process is not primary (tuple already exists and process is already on list)
                if (!_correct[suffixedProcess.Name].Item1.Equals(suffixedProcess)) {
                    Tuple<Process, IList<Process>> tuple = _correct[suffixedProcess.Name];
                    _correct[suffixedProcess.Name] = new Tuple<Process, IList<Process>>(suffixedProcess, tuple.Item2);
                }
            }

            //Console.WriteLine("New leader: " + reply.Url);

            // Send older but possibly unreached requests
            Parallel.ForEach(_fromSelf, request => {
                //DEBUG: Console.WriteLine("Resending " + string.Join(" , ", request.Item1.Select(aa => string.Join("-", aa))));
                _pointToPointLink.Send(process, request);
            });
        }
    }
}
