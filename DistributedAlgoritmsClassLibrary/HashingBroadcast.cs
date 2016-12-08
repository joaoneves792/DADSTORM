using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using TupleMessage = List<IList<string>>;

    public class HashingBroadcast : MutableBroadcast {
        private const string CLASSNAME = "MutableBroadcast";
        private PointToPointLink _pointToPointLink;

        private IDictionary<string, IList<Process>> _processes;

        private readonly int _fieldId;

        public IList<Process> Processes {
            get {
                return _processes
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }


        public HashingBroadcast(PointToPointLink pointToPointLink, int fieldId) {
            _fieldId = fieldId;
            _processes = new Dictionary<string, IList<Process>>();
            _pointToPointLink = pointToPointLink;
        }

        public void Broadcast(Message message) {
            int hash;
            TupleMessage tupleMessage;

            Parallel.ForEach((TupleMessage)message, tuple => {
                hash = tuple[_fieldId].GetHashCode() % _processes.Values.Count;
                tupleMessage = new TupleMessage() { tuple };
                Parallel.ForEach(_processes.Values.Select((list) => list[hash]), process => {
                    _pointToPointLink.Send(process, tupleMessage);
                });
            });
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
