using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary {
    using Message = Object;

    public class EliminateDuplicates : PerfectPointToPointLink {
        private Action<Process, Message> _listener;
        private StubbornPointToPointLink _stubbornPointToPointLink;

        private IProducerConsumerCollection<Tuple<Process, Message>> _delivered;

        public EliminateDuplicates(Process process, Action<Process, Message> listener) {
            _listener = listener;
            _stubbornPointToPointLink = new RetransmitForever(process, Deliver);

            _delivered = new ConcurrentBag<Tuple<Process, Message>>();
        }

        public EliminateDuplicates(Process process, Action<Process, Message> listener, params Process[] otherProcesses) {
            _listener = listener;
            _stubbornPointToPointLink = new RetransmitForever(process, Deliver, otherProcesses);

            _delivered = new ConcurrentBag<Tuple<Process, Message>>();
        }

        public void Send(Process process, Message message) {
            _stubbornPointToPointLink.Send(process, message);
        }

        public void Deliver(Process process, Message message) {
            Tuple<Process, Message> delivered = new Tuple<Process, Message>(process, message);

            if (!_delivered.Contains(delivered)) {
                _delivered.TryAdd(delivered);
                _listener(process, message);
            }
        }

        public void Connect(Process process) {
            _stubbornPointToPointLink.Connect(process);
        }
    }
}
