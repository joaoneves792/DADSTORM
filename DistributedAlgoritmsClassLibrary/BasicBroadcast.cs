using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class BasicBroadcast : BestEffortBroadcast {
        private Action<Process, Message> _listener;
        private PerfectPointToPointLink _perfectPointToPointLink;

        private IProducerConsumerCollection<Process> _processes;

        public BasicBroadcast(Process process, Action<Process, Message> listener) {
            _listener = listener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver);

            _processes = new ConcurrentBag<Process>();
            _processes.TryAdd(process);
        }

        public BasicBroadcast(Process process, Action<Process, Message> listener, params Process[] otherProcesses)
        {
            _listener = listener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver, otherProcesses);

            _processes = new ConcurrentBag<Process>();
            _processes.TryAdd(process);
            foreach (Process otherProcess in otherProcesses) {
                _processes.TryAdd(otherProcess);
            }
        }

        public void Broadcast(Message message) {
            foreach (Process process in _processes) {
                _perfectPointToPointLink.Send(process, message);
            }
        }

        public void Deliver(Process process, Message message) {
            _listener(process, message);
        }

        public void Connect(Process process) {
            _perfectPointToPointLink.Connect(process);
            _processes.TryAdd(process);
        }
    }
}
