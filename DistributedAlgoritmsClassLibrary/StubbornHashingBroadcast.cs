using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class StubbornHashingBroadcast : ReliableBroadcast {
        private Action<Process, Message> _listener;
        private HashingBroadcast _hashingBroadcast;
        private const int TIMER = 5000;
        private IProducerConsumerCollection<Message> _sent;

        public IList<Process> Processes {
            get {
                return _hashingBroadcast.Processes;
            }
        }

        private int _rotation;

        public StubbornHashingBroadcast(Process process, Action<Process, Message> listener, int fieldId) {
            _listener = listener;

            _sent = new ConcurrentBag<Message>();
            _rotation = 0;
            _hashingBroadcast = new HashingBroadcast(process, Deliver, fieldId);
            StartTimer();
        }

        private void Timeout() {
            foreach (Message sent in _sent) {
                _hashingBroadcast.Broadcast(sent, ++_rotation);
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
            _hashingBroadcast.Broadcast(message);
        }

        public void Deliver(Process process, Message message) {
            _listener(process, message);
        }

        public void Connect(Process process) {
            _hashingBroadcast.Connect(process);
        }
    }
}
