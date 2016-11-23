using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using Timestamp = Int32;
    using Value = IList<String>;

    public class ReadWriteEpochConsensus : EpochConsensus {
        private Action<Value> _decideListener;
        private Action<Tuple<Timestamp, Value>> _abortedListener;

        private PerfectPointToPointLink _perfectPointToPointLink;
        private BestEffortBroadcast _bestEffortBroadcast;

        private Tuple<Timestamp, Value> _state;
        private Value _tmpval;
        private IDictionary<Process, Tuple<Timestamp, Value>> _states;
        private int _accepted, _replicationFactor;
        private Timestamp _ets;

        public ReadWriteEpochConsensus (Process process,
                                        Tuple<Timestamp, Value> state,
                                        int replicationFactor,
                                        Timestamp ets,
                                        Action<Value> decideListener,
                                        Action<Tuple<Timestamp, Value>> abortedListener,
                                        params Process[] otherProcesses) {
            _decideListener = decideListener;
            _abortedListener = abortedListener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver, otherProcesses);
            _bestEffortBroadcast = new BasicBroadcast(process, Deliver, otherProcesses);

            _state = state;
            _tmpval = null;
            _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
            _accepted = 0;
            _replicationFactor = replicationFactor;
            _ets = ets;
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
            _perfectPointToPointLink.Send(process, (Message) new Tuple<Signal, Tuple<Timestamp, Value>> (Signal.STATE, _state));
        }

        public void DeliverState(Process process, Tuple<Timestamp, Value> message) {
            _states[process] = message;
            
            Task.Run(() => { lock (_states) { TryWrite(); } });
        }

        private void TryWrite() {
            if (_states.Count > _replicationFactor / 2) {
                _state = Highest(_states);
                if (_state.Item2 != null) {
                    _tmpval = _state.Item2;
                    Console.WriteLine("State: " + String.Join(" - ", _tmpval));
                }
                _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
                _bestEffortBroadcast.Broadcast((Message)new Tuple<Signal, Value>(Signal.WRITE, _tmpval));
            }
        }

        private Tuple<Timestamp, Value> Highest(IDictionary<Process, Tuple<Timestamp, Value>> states) {
            Tuple<Timestamp, Value> state = new Tuple<Timestamp, Value>(-1, null),
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
            Task.Run(() => { lock (_states) { TryDecide(); } });
        }

        private void TryDecide() {
            if (_accepted > _replicationFactor / 2) {
                _accepted = 0;
                _bestEffortBroadcast.Broadcast((Message) new Tuple<Signal, Value>(Signal.DECIDED, _tmpval));
            }
        }

        public void DeliverDecided(Process process, Value message) {
            _decideListener(message);
        }

        public void Abort() {
            _abortedListener(_state);
            //TODO: find a way to halt me
        }
    }
}
