using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LoggingClassLibrary;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RetransmitForever : StubbornPointToPointLink
    {
        private Action<Process, Message> _listener;
        private FairLossPointToPointLink _fairLossPointToPointLink;
        private const int TIMER = 5000;

        private IProducerConsumerCollection<Tuple<Process, Message>> _sent;

        public RetransmitForever(Process process, Action<Process, Message> listener)
        {
            _listener = listener;
            _fairLossPointToPointLink = new RmiNode(process, Deliver);

            _sent = new ConcurrentBag<Tuple<Process, Message>>();
            StartTimer();
        }

        public RetransmitForever(Process process, Action<Process, Message> listener, params Process[] otherProcesses) {
            _listener = listener;
            _fairLossPointToPointLink = new RmiNode(process, Deliver, otherProcesses);

            _sent = new ConcurrentBag<Tuple<Process, Message>>();
            StartTimer();
        }

        public void Timeout() {
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
            _fairLossPointToPointLink.Send(process, message);

            Tuple<Process, Message> sent = new Tuple<Process, Message>(process, message);
            while (!_sent.TryAdd(sent)) {
                Log.WriteLine(LogStatus.CRITICAL, "Message not sent");
            }
        }

        public void Deliver(Process process, Message message) {
            _listener(process, message);
        }
    }
}
