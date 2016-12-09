using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary {
    using Message   = Object;
    using Timestamp = Int32;

    public class EliminateDuplicates : PerfectPointToPointLink {
        private const string CLASSNAME = "EliminateDuplicates";
        private Action<Process, Message> _listener;
        private StubbornPointToPointLink _stubbornPointToPointLink;
        private Process _self;

        private IProducerConsumerCollection<string> _delivered;

        private Timestamp _timestampCounter;

        public EliminateDuplicates(Process process, Action<Process, Message> listener) {
            _listener = listener;
            _self = process;

            _delivered = new ConcurrentBag<string>();

            _timestampCounter = 0;

            _stubbornPointToPointLink = new RetransmitForever(process.Concat(CLASSNAME), Deliver);
        }

        public EliminateDuplicates(Process process, Action<Process, Message> listener, params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _listener = listener;
            _stubbornPointToPointLink = new RetransmitForever(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
            _self = process;

            _delivered = new ConcurrentBag<string>();
        }

        public void Send(Process process, Message message) {
            message = (Message)new Tuple<string, Message>(_self.SuffixedUrl + _timestampCounter++, message);
            _stubbornPointToPointLink.Send(process.Concat(CLASSNAME), message);
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
            _listener(process.Unconcat(CLASSNAME), message);
        }

        public void Connect(Process process) {
            _stubbornPointToPointLink.Connect(process.Concat(CLASSNAME));
        }
    }
}
