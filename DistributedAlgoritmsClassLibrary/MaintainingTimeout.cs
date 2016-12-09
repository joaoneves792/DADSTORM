using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class MaintainingTimeout : EventuallyPerfectFailureDetector {
        private Action<Process> _suspectListener,
                                _restoreListener;
        private FairLossPointToPointLink _fairLossPointToPointLink;
        private const int TIMER = 5000;
        private const string CLASSNAME = "EventuallyPerfectFailureDetector";
        private IList<Process> _processes;
        private IProducerConsumerCollection<Process> _alive;
        private IList<Process> _suspected, _pending;
        private int _delay;

        public MaintainingTimeout(Process process,
                                 Action<Process> suspectListener,
                                 Action<Process> restoreListener) {
            _suspectListener = suspectListener;
            _restoreListener = restoreListener;
            _processes = new List<Process>();

            _alive = new ConcurrentBag<Process>();
            _suspected = new List<Process>();
            _pending = new List<Process>();
            _delay = TIMER;

            _fairLossPointToPointLink = new RemotingNode(process.Concat(CLASSNAME), Deliver);
            StartTimer();
        }

        private void Timeout() {
            lock (_alive) {
                foreach (Process process in _processes) {
                    if (!_alive.Contains(process) && !_suspected.Contains(process) && !_pending.Contains(process)) {
                        _suspected.Add(process);
                        _suspectListener(process);
                    } else if (_alive.Contains(process) && _suspected.Contains(process) && !_pending.Contains(process)) {
                        _suspected.Remove(process);
                        _restoreListener(process);
                    }
                    if (_pending.Contains(process)) {
                        _pending.Remove(process);
                    }
                }
                _alive = new ConcurrentBag<Process>();
            }
            foreach (Process process in _processes) {
                _fairLossPointToPointLink.Send(process, Signal.HEARTBEAT_REQUEST);
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
            _fairLossPointToPointLink.Send(process, (Message) Signal.HEARTBEAT_REPLY);
        }

        public void DeliverHeartbeatReply(Process process) {
            lock (_alive) {
                if (_alive.Contains(process)) {
                    return;
                }
                _alive.TryAdd(process);
            }
        }

        public void Submit(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            _pending.Add(suffixedProcess);
            _processes.Add(suffixedProcess);
            _fairLossPointToPointLink.Connect(suffixedProcess);
        }
    }
}
