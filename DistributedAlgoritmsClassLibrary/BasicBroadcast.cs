using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class BasicBroadcast : BestEffortBroadcast {
        private const string CLASSNAME = "BasicBroadcast";
        private Action<Process, Message> _listener;
        private PerfectPointToPointLink _perfectPointToPointLink;

        private IList<Process> _processes;

        public IList<Process> Processes {
            get { return _processes; }
        }

        public BasicBroadcast(Process process, Action<Process, Message> listener, params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _listener = listener;

            _processes = new List<Process>();
            _processes.Add(process.Concat(CLASSNAME));
            foreach (Process otherProcess in suffixedProcesses) {
                _processes.Add(otherProcess);
            }

            _perfectPointToPointLink = new EliminateDuplicates(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
        }

        public void Broadcast(Message message) {
            foreach (Process process in _processes) {
                _perfectPointToPointLink.Send(process, message);
            }
        }

        public void Deliver(Process process, Message message) {
            _listener(process.Unconcat(CLASSNAME), message);
        }
    }
}
