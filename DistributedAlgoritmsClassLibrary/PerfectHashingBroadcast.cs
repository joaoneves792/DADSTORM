using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary {
    using Message   = Object;
    using Timestamp = Int32;

    public class PerfectHashingBroadcast : ReliableBroadcast {
        private Action<Process, Message> _listener;
        private StubbornHashingBroadcast _stubbornHashingBroadcast;
        private Process _self;

        private IProducerConsumerCollection<string> _delivered;

        private Timestamp _timestampCounter;

        public IList<Process> Processes {
            get {
                return _stubbornHashingBroadcast.Processes;
            }
        }

        public PerfectHashingBroadcast(Process process, Action<Process, Message> listener, int fieldId) {
            _listener = listener;
            _self = process;

            _delivered = new ConcurrentBag<string>();

            _timestampCounter = 0;

            _stubbornHashingBroadcast = new StubbornHashingBroadcast(process, Deliver, fieldId);
        }

        public void Broadcast(Message message) {
            message = new Tuple<string, Message>(_self.SuffixedUrl + _timestampCounter++, message);
            _stubbornHashingBroadcast.Broadcast(message);
        }

        public void Deliver(Process process, Message message) {
            Tuple<string, Message> delivered = (Tuple<string, Message>)message;

            lock (_delivered) {
                if (_delivered.Contains(delivered.Item1)) {
                    return;
                }

                message = delivered.Item2;
                _delivered.TryAdd(delivered.Item1);
            }
            _listener(process, message);
        }

        public void Connect(Process process) {
            _stubbornHashingBroadcast.Connect(process);
        }
    }
}
