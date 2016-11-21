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

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RemotingNode : MarshalByRefObject, FairLossPointToPointLink
    {
        private static int TIMER = 100;

        private readonly Process _process;
        private Action<Process, Message> _listener;
        private IDictionary<Process, FairLossPointToPointLink> _fairLossPointToPointLinks;

        public RemotingNode(Process process, Action<Process, Message> listener) {
            _process = process;
            _listener = listener;
            _fairLossPointToPointLinks = new Dictionary<Process, FairLossPointToPointLink>();

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = process.Port;
            try {
                TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
                ChannelServices.RegisterChannel(channel, true);

                RemotingServices.Marshal(
                    this,
                    process.ServiceName,
                    typeof(FairLossPointToPointLink));
            }
            catch (SocketException) { }
        }

        public RemotingNode(Process process, Action<Process, Message> listener, params Process[] otherProcesses) : this(process, listener) {
            foreach (Process otherProcess in otherProcesses) {
                Connect(otherProcess);
            }
        }

        public void Connect(Process process) {
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
            } catch (SocketException) {
                new Thread(() => {
                    Thread.Sleep(TIMER);
                    Connect(process);
                }).Start();
            }
        }

        public void Reconnect(Process process) {
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
            } catch (SocketException) { }
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
                    Console.WriteLine(ex);
                    Reconnect(process);
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
    }
}