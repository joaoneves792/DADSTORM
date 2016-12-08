using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class PrimaryBroadcast : MutableBroadcast {
        private const string CLASSNAME = "MutableBroadcast";
        private PointToPointLink _pointToPointLink;

        private IDictionary<string, Tuple<Process, IList<Process>>> _processes;

        public IList<Process> Processes {
            get {
                return _processes
                    .Values
                    .Select((tuple) => tuple.Item2)
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }

        public PrimaryBroadcast(PointToPointLink pointToPointLink) {
            _processes = new Dictionary<string, Tuple<Process, IList<Process>>>();
            _pointToPointLink = pointToPointLink;
        }

        public void Broadcast(Message message) {
            Parallel.ForEach(_processes.Values.Select((tuple) => tuple.Item1), process => {
                _pointToPointLink.Send(process, message);
            });
        }

        public void Connect(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            lock (_processes) {
                //Create new tuple if tuple does not exist
                if (!_processes.ContainsKey(suffixedProcess.Name)) {
                    _processes.Add(suffixedProcess.Name, new Tuple<Process, IList<Process>>(suffixedProcess, new List<Process>(){ suffixedProcess }));
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (tuple already exists)
                if (!_processes[suffixedProcess.Name].Item2.Contains(suffixedProcess)) {
                    _processes[suffixedProcess.Name].Item2.Add(suffixedProcess);
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Change primary if process is not primary (tuple already exists and process is already on list)
                if (!_processes[suffixedProcess.Name].Item1.Equals(suffixedProcess)) {
                    Tuple<Process, IList<Process>> tuple = _processes[suffixedProcess.Name];
                    _processes[suffixedProcess.Name] = new Tuple<Process, IList<Process>>(suffixedProcess, tuple.Item2);
                }
            }
        }
    }
}
