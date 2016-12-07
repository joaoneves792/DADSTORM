using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RetransmitForever : StubbornPointToPointLink {
        private Action<Process, Message> _listener;
        private FairLossPointToPointLink _fairLossPointToPointLink;
        private const int TIMER = 5000;
        private const string CLASSNAME = "RetransmitForever";
        private IProducerConsumerCollection<Tuple<Process, Message>> _sent;

        public RetransmitForever(Process process, Action<Process, Message> listener) {
            _listener = listener;

            _sent = new ConcurrentBag<Tuple<Process, Message>>();

            _fairLossPointToPointLink = new RemotingNode(process.Concat(CLASSNAME), Deliver);
            StartTimer();
        }

        public RetransmitForever(Process process, Action<Process, Message> listener, params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _listener = listener;
            _fairLossPointToPointLink = new RemotingNode(process.Concat(CLASSNAME), Deliver, suffixedProcesses);

            _sent = new ConcurrentBag<Tuple<Process, Message>>();
            StartTimer();
        }

        private void Timeout() {
            foreach (Tuple<Process, Message> sent in _sent) {
                _fairLossPointToPointLink.Send(sent.Item1, sent.Item2);
            }
            StartTimer();
        }

        private void StartTimer() {
            new Thread(() => {
                Thread.Sleep(TIMER);
                Timeout();
            }).Start();
        }

        public void Send(Process process, Message message) {
            _fairLossPointToPointLink.Send(process.Concat(CLASSNAME), message);

            Tuple<Process, Message> sent = new Tuple<Process, Message>(process.Concat(CLASSNAME), message);
            _sent.TryAdd(sent);
        }

        public void Deliver(Process process, Message message) {
            _listener(process.Unconcat(CLASSNAME), message);
        }

        public void Connect(Process process) {
            _fairLossPointToPointLink.Connect(process.Concat(CLASSNAME));
        }
    }
}
