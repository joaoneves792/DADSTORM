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
                                                  Action<Process> listener,
                                                  params Process[] otherProcesses) {
            _listener = listener;
            _eventuallyPerfectFailureDetector = new IncreasingTimeout(process, Suspect, Restore, otherProcesses);
            _processes = new ConcurrentBag<Process>();
            foreach (Process otherProcess in otherProcesses) {
                _processes.TryAdd(otherProcess);
            }
            _processes.TryAdd(process);

            _leader = null;
            _suspected = new List<Process>();
            Task.Run(() => { TryAbort(); });
            Task.Run(() => { TryTrust(); });
        }

        public void Suspect(Process process) {
            _suspected.Add(process);
            TryAbort();
            Task.Run(() => { TryAbort(); });
            Task.Run(() => { TryTrust(); });
        }

        public void Restore(Process process) {
            _suspected.Remove(process);
            Task.Run(() => { TryAbort(); });
            Task.Run(() => { TryTrust(); });
        }

        public void TryTrust() {
            Process usurper = _processes.Except(_suspected).Max();
            if (!usurper.Equals(_leader)) {
                _leader = usurper;
                _listener(_leader);
            }
        }

        public void TryAbort() {
            if(_processes.Except(_suspected).Count() == 0) {
                _eventuallyPerfectFailureDetector = null;
                Console.WriteLine("ERROR");
                Console.ReadLine();
            }
        }
    }
}
