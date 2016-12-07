using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using CommonTypesLibrary;
    using Message = Object;

    public class RemotingNode : MarshalByRefObject, FairLossPointToPointLink
    {
        private const int TIMER = 100;

        private readonly Process _process;
        private Action<Process, Message> _listener;
        private IDictionary<string, FairLossPointToPointLink> _fairLossPointToPointLinks;

        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenSends, _frozenDelivers;
        private Action<Process, Message> _send, _deliver;

        public RemotingNode(Process process, Action<Process, Message> listener) {
            _process = process;
            _listener = listener;
            _send = UnfrozenSend;
            _deliver = UnfrozenDeliver;

            _frozenSends = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenDelivers = new ConcurrentBag<Tuple<Process, Message>>();

            _fairLossPointToPointLinks = new Dictionary<string, FairLossPointToPointLink>();

            //Console.WriteLine("\nSubmit:");
            //Console.WriteLine("Service:\n" + process.ServiceName);
            //Console.WriteLine("Hashcode:\n" + process.Url.GetHashCode());
            //Thread.Sleep(2000);

            RemotingServices.Marshal(
                this,
                process.SuffixedServiceName,
                typeof(FairLossPointToPointLink));
        }

        public RemotingNode(Process process, Action<Process, Message> listener, params Process[] otherProcesses) : this(process, listener) {
            Thread.Sleep(1000);
            foreach (Process otherProcess in otherProcesses) {
                Connect(otherProcess);
            }
        }

        public void Connect(Process process) {
            FairLossPointToPointLink fairLossPointToPointLink = SubmitProcessAsFairLossPointToPointLinkNode(process);

            try {
                fairLossPointToPointLink.SubmitProcessAsFairLossPointToPointLinkNode(_process);
            } catch (Exception) {
                new Thread(() => {
                    //Console.WriteLine("------------------------------------------");
                    Thread.Sleep(TIMER);
                    Connect(process);
                }).Start();
            }
        }

        public void Reconnect(Process process) {
            FairLossPointToPointLink fairLossPointToPointLink = SubmitProcessAsFairLossPointToPointLinkNode(process);

            Task.Run(() => {
                fairLossPointToPointLink.SubmitProcessAsFairLossPointToPointLinkNode(_process);
            });
        }

        public FairLossPointToPointLink SubmitProcessAsFairLossPointToPointLinkNode(Process process) {
            //Console.WriteLine("\nConnect by:");
            //Console.WriteLine("Service:\n" + process.ServiceName);
            //Console.WriteLine("Hashcode:\n" + process.Url.GetHashCode());
            //Thread.Sleep(2000);

            FairLossPointToPointLink fairLossPointToPointLink = (FairLossPointToPointLink) Activator.GetObject(
                typeof(FairLossPointToPointLink),
                process.SuffixedUrl);

            if (_fairLossPointToPointLinks.ContainsKey(process.SuffixedUrl)) {
                _fairLossPointToPointLinks[process.SuffixedUrl] = fairLossPointToPointLink;
            } else {
                _fairLossPointToPointLinks.Add(process.SuffixedUrl, fairLossPointToPointLink);
            }

            return fairLossPointToPointLink;
        }

        public void Send(Process process, Message message) {
            CheckFrozenFlag();
            _send(process, message);
        }

        public void Deliver(Process process, Message message) {
            CheckFrozenFlag();
            _deliver(process, message);
        }

        private void CheckFrozenFlag() {
            if(Flag.Frozen) {
                Freeze();
            } else {
                Unfreeze();
            }
        }

        public void UnfrozenSend(Process process, Message message) {
            //Console.WriteLine("\nSend to:");
            //Console.WriteLine("Service:\n" + process.ServiceName);
            //Console.WriteLine("Hashcode:\n" + process.Url.GetHashCode());
            //Console.WriteLine("List:\n" + string.Join("\n", _fairLossPointToPointLinks.Keys.Select((aaa) => aaa.GetHashCode())));
            //Thread.Sleep(2000);

            Task.Run(() => {
                _fairLossPointToPointLinks[process.SuffixedUrl].Deliver(_process, message);
            }).ContinueWith(task => {
                //Handles remote exception
                task.Exception.Handle(ex => {
                    //Console.WriteLine("------------------------------------------");
                    Thread.Sleep(TIMER);
                    Reconnect(process);
                    return true;
                });
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void UnfrozenDeliver(Process process, Message message) {
            _listener(process, message);
        }

        public void Freeze() {
            _send = FrozenSend;
            _deliver = FrozenDeliver;
        }

        public void Unfreeze() {
            _send = UnfrozenSend;
            _deliver = UnfrozenDeliver;

            Parallel.ForEach(_frozenSends, (send) => {
                _send(send.Item1, send.Item2);
            });
            Parallel.ForEach(_frozenDelivers, (deliver) => {
                    _deliver(deliver.Item1, deliver.Item2);
            });

            _frozenSends = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenDelivers = new ConcurrentBag<Tuple<Process, Message>>();
        }

        private void FrozenSend(Process process, Message message) {
			Tuple<Process, Message> send = new Tuple<Process, Message>(process, message);
			_frozenSends.TryAdd(send);
		}

		private void FrozenDeliver(Process process, Message message) {
			Tuple<Process, Message> deliver = new Tuple<Process, Message>(process, message);
			_frozenDelivers.TryAdd(deliver);
		}
    }
}