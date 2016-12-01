﻿using System;
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
    using CommonTypesLibrary;
    using Message = Object;

    public class RemotingNode : MarshalByRefObject, FairLossPointToPointLink
    {
        private const int TIMER = 100;
        private static int _channelIdCounter = 0;

        private readonly int _channelId;
        private readonly Process _process;
        private Action<Process, Message> _listener;
        private IDictionary<Process, FairLossPointToPointLink> _fairLossPointToPointLinks;

        private IProducerConsumerCollection<Tuple<Process, Message>> _frozenSends, _frozenDelivers;
        private Action<Process, Message> _freezableSend, _freezableDeliver;

        public RemotingNode(Process process, Action<Process, Message> listener) {
            _channelId = _channelIdCounter++;
            _process = process;
            _listener = listener;
            _freezableSend = FreezableSend;
            _freezableDeliver = FreezableDeliver;

            _frozenSends = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenDelivers = new ConcurrentBag<Tuple<Process, Message>>();

            _fairLossPointToPointLinks = new Dictionary<Process, FairLossPointToPointLink>();

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = process.Port;
            try {
                TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
                ChannelServices.RegisterChannel(channel, true);
            } catch { }

            RemotingServices.Marshal(
                this,
                process.ServiceName + _channelId,
                typeof(FairLossPointToPointLink));
        }

        public RemotingNode(Process process, Action<Process, Message> listener, params Process[] otherProcesses) : this(process, listener) {
            Thread.Sleep(1000);
            foreach (Process otherProcess in otherProcesses) {
                Connect(otherProcess);
            }
        }

        public void Connect(Process process) {
            FairLossPointToPointLink fairLossPointToPointLink = (FairLossPointToPointLink) Activator.GetObject(
                typeof(FairLossPointToPointLink),
                process.Url + _channelId);

            if (_fairLossPointToPointLinks.ContainsKey(process)) {
                _fairLossPointToPointLinks[process] = fairLossPointToPointLink;
            } else {
                _fairLossPointToPointLinks.Add(process, fairLossPointToPointLink);
            }

            try {
                fairLossPointToPointLink.Anchor(_process);
            } catch (Exception) {
                new Thread(() => {
                    Thread.Sleep(TIMER);
                    Connect(process);
                }).Start();
            }
        }

        public void Reconnect(Process process) {
            FairLossPointToPointLink fairLossPointToPointLink = (FairLossPointToPointLink) Activator.GetObject(
                typeof(FairLossPointToPointLink),
                process.Url + _channelId);

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
                process.Url + _channelId);
            //@"tcp://" + serviceURL + ":" + PORT + "/" + SERVICE_NAME);

            if (_fairLossPointToPointLinks.ContainsKey(process)) {
                _fairLossPointToPointLinks[process] = fairLossPointToPointLink;
            } else {
                _fairLossPointToPointLinks.Add(process, fairLossPointToPointLink);
            }
        }

        public void Send(Process process, Message message) {
            if(Flag.Frozen) {
                Freeze();
            } else {
                Unfreeze();
            }
            _freezableSend(process, message);
        }

        public void Deliver(Process process, Message message) {
            if(Flag.Frozen) {
                Freeze();
            } else {
                Unfreeze();
            }
            _freezableDeliver(process, message);
        }

        public void FreezableSend(Process process, Message message) {
            //Console.WriteLine(_channelId + ": from " + _process.Name + " to " + process.Name);
            Task.Run(() => {
                _fairLossPointToPointLinks[process].Deliver(_process, message);
            }).ContinueWith(task => {
                //Handles remote exception
                task.Exception.Handle(ex => {
                    //Console.WriteLine(ex);
                    Reconnect(process);
                    return true;
                });
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void FreezableDeliver(Process process, Message message) {
            //Console.WriteLine(_channelId + ": from " + process.Name + " to " + _process.Name);
            //try {
                _listener(process, message);
            //} catch () {
            //    Reconnect(process);
            //}
        }

        public void Freeze() {
            _freezableSend = StoreSend;
            _freezableDeliver = StoreDeliver;
        }

        public void Unfreeze() {
            _freezableSend = FreezableSend;
            _freezableDeliver = FreezableDeliver;

            foreach (Tuple<Process, Message> send in _frozenSends)
            {
                new Thread(() => {
                    _freezableSend(send.Item1, send.Item2);
                }).Start();
            }
            foreach (Tuple<Process, Message> deliver in _frozenDelivers)
            {
                new Thread(() => {
                    _freezableDeliver(deliver.Item1, deliver.Item2);
                }).Start();
            }

            _frozenSends = new ConcurrentBag<Tuple<Process, Message>>();
            _frozenDelivers = new ConcurrentBag<Tuple<Process, Message>>();
        }

        private void StoreSend(Process process, Message message) {
			Tuple<Process, Message> send = new Tuple<Process, Message>(process, message);
			_frozenSends.TryAdd(send);
		}

		private void StoreDeliver(Process process, Message message) {
			Tuple<Process, Message> deliver = new Tuple<Process, Message>(process, message);
			_frozenDelivers.TryAdd(deliver);
		}
    }
}