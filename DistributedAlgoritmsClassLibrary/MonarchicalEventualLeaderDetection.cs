using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    public class MonarchicalEventualLeaderDetection : EventualLeaderDetector {
        private Action<Process> _listener;
        private EventuallyPerfectFailureDetector _eventuallyPerfectFailureDetector;

        private IProducerConsumerCollection<Process> _processes;
        private IList<Process> _suspected;
        private Process _leader;

        public MonarchicalEventualLeaderDetection(Process process,
                                                  Action<Process> listener) {
            _listener = listener;
            _eventuallyPerfectFailureDetector = new IncreasingTimeout(process, Suspect, Restore);
            _processes = new ConcurrentBag<Process>();

            _suspected = new List<Process>();
            _leader = null;
        }

        public MonarchicalEventualLeaderDetection(Process process,
                                                  Action<Process> listener,
                                                  params Process[] otherProcesses) {
            _listener = listener;
            _eventuallyPerfectFailureDetector = new IncreasingTimeout(process, Suspect, Restore);
            foreach (Process otherProcess in otherProcesses) {
                _processes.TryAdd(otherProcess);
            }

            _suspected = new List<Process>();
            _leader = _processes.First();
        }

        public void Suspect(Process process) {
            _suspected.Add(process);
            TryTrust();
        }

        public void Restore(Process process) {
            _suspected.Remove(process);
            TryTrust();
        }

        public void TryTrust() {
            Process usurper = _processes.Intersect(_suspected).Max();
            if (_leader.Rank != usurper.Rank) {
                _leader = usurper;
                _listener(_leader);
            }
        }

        public void Connect(Process process)  {
            _leader = process;
            _eventuallyPerfectFailureDetector.Connect(process);
            _processes.TryAdd(process);
        }
    }
}
