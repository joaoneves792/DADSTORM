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

    public class IncreasingTimeout : EventuallyPerfectFailureDetector {
        private Action<Process> _suspectListener,
                                _restoreListener;
        private PerfectPointToPointLink _perfectPointToPointLink;
        private const int TIMER = 5000;

        private Process[] _processes;
        private IProducerConsumerCollection<Process> _alive;
        private IList<Process> _suspected;
        private int _delay;

        public IncreasingTimeout(Process process,
                                 Action<Process> suspectListener,
                                 Action<Process> restoreListener,
                                 params Process[] otherProcesses) {
            _suspectListener = suspectListener;
            _restoreListener = restoreListener;
            _processes = otherProcesses;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver, _processes);

            _alive = new ConcurrentBag<Process>();
            foreach (Process otherProcess in otherProcesses) {
                _alive.TryAdd(otherProcess);
            }
            _suspected = new List<Process>();
            _delay = TIMER;
            StartTimer();
        }

        private void Timeout() {
            if(_alive.Count + _suspected.Count > 0) {
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
                _perfectPointToPointLink.Send(process, (Message)new HeartbeatRequest());
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
            if (message is HeartbeatRequest) {
                Deliver(process, (HeartbeatRequest)message);
            } else if (message is HeartbeatReply) {
                Deliver(process, (HeartbeatReply)message);
            }
        }

        public void Deliver(Process process, HeartbeatRequest heartbeatRequest) {
            _perfectPointToPointLink.Send(process, (Message)new HeartbeatReply());
        }

        public void Deliver(Process process, HeartbeatReply heartbeatReply) {
            Console.WriteLine("added " + process);
            _alive.TryAdd(process);
        }
    }
}
