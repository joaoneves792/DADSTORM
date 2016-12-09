using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class StubbornRandomBroadcast : ReliableBroadcast {
        private Action<Process, Message> _listener;
        private RandomBroadcast _randomBroadcast;
        private const int TIMER = 5000;
        private IProducerConsumerCollection<Message> _sent;

        public IList<Process> Processes {
            get {
                return _randomBroadcast.Processes;
            }
        }

        public StubbornRandomBroadcast(Process process, Action<Process, Message> listener) {
            _listener = listener;

            _sent = new ConcurrentBag<Message>();

            _randomBroadcast = new RandomBroadcast(process, Deliver);
            StartTimer();
        }

        private void Timeout() {
            foreach (Message sent in _sent) {
                _randomBroadcast.Broadcast(sent);
            }
            StartTimer();
        }

        private void StartTimer() {
            new Thread(() => {
                Thread.Sleep(TIMER);
                Timeout();
            }).Start();
        }

        public void Broadcast(Message message) {
            _sent.TryAdd(message);
            _randomBroadcast.Broadcast(message);
        }

        public void Deliver(Process process, Message message) {
            _listener(process, message);
        }

        public void Connect(Process process) {
            _randomBroadcast.Connect(process);
        }
    }
}

