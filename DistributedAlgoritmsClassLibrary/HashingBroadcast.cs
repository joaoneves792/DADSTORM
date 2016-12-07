using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class HashingBroadcast : MutableBroadcast {
        private const string CLASSNAME = "MutableBroadcast";
        private PointToPointLink _pointToPointLink;

        private IDictionary<string, IList<Process>> _processes;

        public IList<Process> Processes {
            get {
                return _processes
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }

        private int _hashing;

        public HashingBroadcast(PointToPointLink pointToPointLink, int hashing) {
            _hashing = hashing;
            _processes = new Dictionary<string, IList<Process>>();
            _pointToPointLink = pointToPointLink;
        }

        public void Broadcast(Message message) {
            //FIXME:
            foreach (Process process in _processes.Values.Select((list) => list[_hashing])) {
                _pointToPointLink.Send(process, message);
            }
        }

        public void Connect(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            lock (_processes) {
                //Create new list if list does not exist
                if (!_processes.ContainsKey(suffixedProcess.Name)) {
                    _processes.Add(suffixedProcess.Name, new List<Process>() { suffixedProcess });
                    _pointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (list already exists)
                if (!_processes[suffixedProcess.Name].Contains(suffixedProcess)) {
                    _processes[suffixedProcess.Name].Add(suffixedProcess);
                    _pointToPointLink.Connect(suffixedProcess);
                }
            }
        }
    }
}
