using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class IncreasingTimeout : EventuallyPerfectFailureDetector {
        private Action<Process> _suspectListener,
                                _restoreListener;
        private PerfectPointToPointLink _perfectPointToPointLink;
        private const int TIMER = 1000;
        private const string CLASSNAME = "IncreasingTimeout";
        private Process[] _processes;
        private IProducerConsumerCollection<Process> _alive;
        private IList<Process> _suspected;
        private int _delay;

        public IncreasingTimeout(Process process,
                                 Action<Process> suspectListener,
                                 Action<Process> restoreListener,
                                 params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _suspectListener = suspectListener;
            _restoreListener = restoreListener;
            _processes = suffixedProcesses;

            _alive = new ConcurrentBag<Process>();
            foreach (Process otherProcess in suffixedProcesses) {
                _alive.TryAdd(otherProcess);
            }
            _suspected = new List<Process>();
            _delay = TIMER;

            _perfectPointToPointLink = new EliminateDuplicates(process.Concat(CLASSNAME), Deliver, _processes);
            StartTimer();
        }

        private void Timeout() {
            if (_alive.Count + _suspected.Count > 0) {
                _delay += TIMER;
            }
            foreach (Process process in _processes) {
                if (!_alive.Contains(process) && !_suspected.Contains(process)) {
                    _suspected.Add(process);
                    _suspectListener(process);
                } else if (_alive.Contains(process) && _suspected.Contains(process)) {
                    _suspected.Remove(process);
                    _restoreListener(process);
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
                Thread.Sleep(_delay);
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
