using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class ExcludeOnTimeout : PerfectFailureDetector {
        private Action<Process> _listener;
        private PerfectPointToPointLink _perfectPointToPointLink;
        private const int TIMER = 1000;
        private const string CLASSNAME = "ExcludeOnTimeout";
        private Process[] _processes;
        private IProducerConsumerCollection<Process> _alive;
        private IList<Process> _detected;

        public ExcludeOnTimeout(Process process,
                                 Action<Process> listener,
                                 params Process[] otherProcesses) {
            _listener = listener;
            _processes = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();
            _perfectPointToPointLink = new EliminateDuplicates(process.Concat(CLASSNAME), Deliver, _processes);

            _alive = new ConcurrentBag<Process>();
            foreach (Process otherProcess in _processes) {
                _alive.TryAdd(otherProcess);
            }
            _detected = new List<Process>();
            StartTimer();
        }

        private void Timeout() {
            foreach (Process process in _processes) {
                if (!_alive.Contains(process) && !_detected.Contains(process)) {
                    _detected.Add(process);
                    _listener(process.Unconcat(CLASSNAME));
                }
            }
            _alive = new ConcurrentBag<Process>();
            foreach (Process process in _processes) {
                _perfectPointToPointLink.Send(process, Signal.HEARTBEAT_REQUEST);
            }
            StartTimer();
        }

        private void StartTimer() {
            new Thread(() => {
                Thread.Sleep(TIMER);
                Timeout();
            }).Start();
        }

        public void Deliver(Process process, Message message) {
            switch((Signal)message) {
                case Signal.HEARTBEAT_REQUEST:
                    DeliverHeartbeatRequest(process);
                    break;
                case Signal.HEARTBEAT_REPLY:
                    DeliverHeartbeatReply(process);
                    break;
            }
        }

        public void DeliverHeartbeatRequest(Process process) {
            _perfectPointToPointLink.Send(process, (Message) Signal.HEARTBEAT_REPLY);
        }

        public void DeliverHeartbeatReply(Process process) {
            _alive.TryAdd(process);
        }
    }
}
