using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using Timestamp = Int32;

    public class ReadWriteEpochConsensus<Value> : EpochConsensus<Value> {
        private const string CLASSNAME = "ReadWriteEpochConsensus";
        private Action<Value> _decideListener;
        private Action<Tuple<Timestamp, Value>> _abortedListener;

        private PerfectPointToPointLink _perfectPointToPointLink;
        private BestEffortBroadcast _bestEffortBroadcast;

        private Tuple<Timestamp, Value> _state;
        private Value _tmpval;
        private IDictionary<Process, Tuple<Timestamp, Value>> _states;
        private int _accepted, _replicationFactor;
        private Timestamp _ets;

        private object _acceptedLock;

        public ReadWriteEpochConsensus (Process process,
                                        Tuple<Timestamp, Value> state,
                                        int replicationFactor,
                                        Timestamp ets,
                                        Action<Value> decideListener,
                                        Action<Tuple<Timestamp, Value>> abortedListener,
                                        params Process[] otherProcesses) {
            _acceptedLock = new Object();

            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _decideListener = decideListener;
            _abortedListener = abortedListener;

            _state = state;
            _tmpval = default(Value);
            _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
            _accepted = 0;
            _replicationFactor = replicationFactor;
            _ets = ets;

            _perfectPointToPointLink = new EliminateDuplicates(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
            _bestEffortBroadcast = new BasicBroadcast(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
        }

        public void Propose(Value value) {
            _tmpval = value;
            _bestEffortBroadcast.Broadcast(Signal.READ);
        }

        public void Deliver(Process process, Message message) {
            if (message is Signal) {
                switch ((Signal)message) {
                    case Signal.READ:
                        DeliverRead(process);
                        break;
                    case Signal.ACCEPT:
                        DeliverAccept(process);
                        break;
                }
            } else if (message is Tuple<Signal, Value>) {
                switch (((Tuple<Signal, Value>)message).Item1) {
                    case Signal.WRITE:
                        DeliverWrite(process, ((Tuple<Signal, Value>)message).Item2);
                        break;
                    case Signal.DECIDED:
                        DeliverDecided(process, ((Tuple<Signal, Value>)message).Item2);
                        break;
                }
            } else if(message is Tuple<Signal, Tuple<Timestamp, Value>>) {
                DeliverState(process, ((Tuple<Signal, Tuple < Timestamp, Value >>)message).Item2);
            }
        }

        public void DeliverRead(Process process) {
            _perfectPointToPointLink.Send(process, new Tuple<Signal, Tuple<Timestamp, Value>> (Signal.STATE, _state));
        }

        public void DeliverState(Process process, Tuple<Timestamp, Value> message) {
            _states[process] = message;
            
            TryWrite();
        }

        private void TryWrite() {
            lock (_states) {
                if (!(_states.Count > _replicationFactor / 2)) {
                    return;
                }
                _state = Highest(_states);
                if (_state.Item2 != null) {
                    _tmpval = _state.Item2;
                }
                _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
            }

            _bestEffortBroadcast.Broadcast(new Tuple<Signal, Value>(Signal.WRITE, _tmpval));
        }

        private Tuple<Timestamp, Value> Highest(IDictionary<Process, Tuple<Timestamp, Value>> states) {
            Tuple<Timestamp, Value> state = new Tuple<Timestamp, Value>(-1, default(Value)),
                                    keyValuePairValue;
            foreach (KeyValuePair<Process, Tuple<Timestamp, Value>> keyValuePair in states) {
                keyValuePairValue = keyValuePair.Value;
                if (keyValuePairValue.Item1 > state.Item1) {
                    state = keyValuePairValue;
                }
            }
            return state;
        }

        public void DeliverWrite(Process process, Value message) {
            _state = new Tuple<Timestamp, Value>(_ets, message);
            _bestEffortBroadcast.Broadcast(Signal.ACCEPT);
        }

        public void DeliverAccept(Process process) {
            _accepted++;
            TryDecide();
        }

        private void TryDecide() {
            lock (_acceptedLock) {
                if (!(_accepted > _replicationFactor / 2 && _tmpval != null)) {
                    return;
                }
                _accepted = 0;
            }

            _bestEffortBroadcast.Broadcast(new Tuple<Signal, Value>(Signal.DECIDED, _tmpval));
        }

        public void DeliverDecided(Process process, Value message) {
            _decideListener(message);
        }

        public void Abort() {
            //Halt algorithm operation
            _replicationFactor *= 3;
            _abortedListener(_state);
        }
    }
}
