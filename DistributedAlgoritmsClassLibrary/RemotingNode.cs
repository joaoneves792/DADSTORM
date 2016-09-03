using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Runtime.Serialization.Formatters;

using LoggingClassLibrary;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RemotingNode : MarshalByRefObject, FairLossPointToPointLink
    {
        private readonly Process _process;
        private Action<Process, Message> _listener, _frozenListener;
        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenRequests;
        private IDictionary<Process, FairLossPointToPointLink> _fairLossPointToPointLinks;

        public RemotingNode(Process process, Action<Process, Message> listener) {
            Log.Write(LogStatus.DEBUG, "Initializing process " + process.ToString() + "...");

            _process = process;
            _listener = listener;
            _frozenListener = listener;
            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
            _fairLossPointToPointLinks = new Dictionary<Process, FairLossPointToPointLink>();

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = process.Port;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(
                this,
                process.ServiceName,
                typeof(FairLossPointToPointLink));

            Log.WriteDone(LogStatus.DEBUG);
        }

        public RemotingNode(Process process, Action<Process, Message> listener, params Process[] otherProcesses) : this(process, listener) {
            foreach (Process otherProcess in otherProcesses) {
                Connect(otherProcess);
            }
        }

        public void Connect(Process process) {
            Log.Write(LogStatus.DEBUG, "Connectiong to process " + process.ToString() + "...");

            FairLossPointToPointLink fairLossPointToPointLink = (FairLossPointToPointLink) Activator.GetObject(
                typeof(FairLossPointToPointLink),
                process.Url);

            if (_fairLossPointToPointLinks.ContainsKey(process)) {
                _fairLossPointToPointLinks[process] = fairLossPointToPointLink;
            } else {
                _fairLossPointToPointLinks.Add(process, fairLossPointToPointLink);
            }

            try {
                fairLossPointToPointLink.Anchor(_process);
            }
            catch (SocketException) { }

            Log.WriteDone(LogStatus.DEBUG);
        }

        public void Anchor(Process process) {
            FairLossPointToPointLink fairLossPointToPointLink = (FairLossPointToPointLink) Activator.GetObject(
                typeof(FairLossPointToPointLink),
                process.Url);
            //@"tcp://" + serviceURL + ":" + PORT + "/" + SERVICE_NAME);

            if (_fairLossPointToPointLinks.ContainsKey(process)) {
                _fairLossPointToPointLinks[process] = fairLossPointToPointLink;
            } else {
                _fairLossPointToPointLinks.Add(process, fairLossPointToPointLink);
            }
        }

        public void Send(Process process, Message message) {
            Task.Run(() => {
                _fairLossPointToPointLinks[process].Deliver(_process, message);
            }).ContinueWith(task => {
                //Handles remote exception
                task.Exception.Handle(ex => {
                    Connect(process);
                    return true;
                });
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Deliver(Process process, Message message) {
            //try {
                _listener(process, message);
            //} catch () {
            //    Reconnect(process);
            //}
        }

        public void Crash() {
            //TODO: Implement method after knowing all requirements
        }

        public void Recover() {
            //TODO: Implement method after knowing all requirements
        }

        public void Freeze() {
            _listener = (process, message) => {
                Tuple<Process, Message> request = new Tuple<Process, Message>(process, message);
                while (!_frozenRequests.TryAdd(request)) {
                    Log.WriteLine(LogStatus.CRITICAL, "Message not froze");
                }
                Log.WriteLine(LogStatus.DEBUG, _frozenRequests.Count + " frozen requests");
            };

            Log.WriteLine(LogStatus.DEBUG, "FREEZE!");
        }

        public void Unfreeze() {
            Log.WriteLine(LogStatus.DEBUG, "UNFREEZE!");

            _listener = _frozenListener;

            foreach (Tuple<Process, Message> request in _frozenRequests) {
                Task.Run(() => {
                    _listener(request.Item1, request.Item2);
                });
            }
            _frozenRequests = new ConcurrentBag<Tuple<Process, Message>>();
        }
    }
}