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

        private IProducerConsumerCollection<Process> _processes,
                                                     _alive;
        private IList<Process> _suspected;
        private int _delay;

        public IncreasingTimeout(Process process,
                         Action<Process> suspectListener,
                         Action<Process> restoreListener) {
            _suspectListener = suspectListener;
            _restoreListener = restoreListener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver);
            _processes = new ConcurrentBag<Process>();

            _alive = new ConcurrentBag<Process>();
            _suspected = new List<Process>();
            _delay = TIMER;
            StartTimer();
        }

        public IncreasingTimeout(Process process,
                                 Action<Process> suspectListener,
                                 Action<Process> restoreListener,
                                 params Process[] otherProcesses) {
            _suspectListener = suspectListener;
            _restoreListener = restoreListener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver);
            foreach (Process otherProcess in otherProcesses) {
                _processes.TryAdd(otherProcess);
            }

            _alive = _processes;
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
                _perfectPointToPointLink.Send(process, (Message)new HeartbeatRequest());
            }
            _alive = new ConcurrentBag<Process>();
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
            Console.WriteLine("request");
            _perfectPointToPointLink.Send(process, (Message)new HeartbeatReply());
        }

        public void Deliver(Process process, HeartbeatReply heartbeatReply) {
            Console.WriteLine("reply");
            _alive.TryAdd(process);
        }

        public void Connect(Process process) {
            _perfectPointToPointLink.Connect(process);
            _processes.TryAdd(process);
            _alive.TryAdd(process);
        }
    }
}
