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
                                        Action<Tuple<Timestamp, Value>> abortedListener) {
            _decideListener = decideListener;
            _abortedListener = abortedListener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver);
            _bestEffortBroadcast = new BasicBroadcast(process, Deliver);

            _state = state;
            _tmpval = null;
            _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
            _accepted = 0;
            _replicationFactor = replicationFactor;
            _ets = ets;
        }

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
            _bestEffortBroadcast.Broadcast((Message) new Read());
        }

        public void Deliver(Process process, Message message) {
            if (message is Read) {
                Deliver(process, (Read)message);
            } else if(message is Tuple<State, Tuple<Timestamp, Value>>) {
                Deliver(process, (Tuple<State, Tuple < Timestamp, Value >>)message);
            } else if (message is Tuple<Write, Value>) {
                Deliver(process, (Tuple<Write, Value>)message);
            } else if (message is Accept) {
                Deliver(process, (Accept)message);
            } else if (message is Tuple<Decided, Value>) {
                Deliver(process, (Tuple<Decided, Value>)message);
            }
        }

        public void Deliver(Process process, Read read) {
            _perfectPointToPointLink.Send(process, (Message) new Tuple<State, Tuple<Timestamp, Value>> (new State(), _state));
        }

        public void Deliver(Process process, Tuple<State, Tuple<Timestamp, Value>> message) {
            _states[process] = message.Item2;
            Task.Run(() => { TryWrite(); });
        }

        private void TryWrite() {
            if (_states.Count > _replicationFactor / 2) {
                _state = Highest(_states);
                if (!_state.Item2.Equals(null)) {
                    _tmpval = _state.Item2;
                }
                _states = new ConcurrentDictionary<Process, Tuple<Timestamp, Value>>();
                _bestEffortBroadcast.Broadcast((Message) new Tuple<Write, Value>(new Write(), _tmpval));
            }
        }

        private Tuple<Timestamp, Value> Highest(IDictionary<Process, Tuple<Timestamp, Value>> states) {
            Tuple<Timestamp, Value> state = new Tuple<Timestamp, Value>(-1, null),
                                    keyValuePairValue;
            foreach(KeyValuePair<Process, Tuple<Timestamp, Value>> keyValuePair in states) {
                keyValuePairValue = keyValuePair.Value;
                if (keyValuePairValue.Item1 > state.Item1) {
                    state = keyValuePairValue;
                }
            }
            return state;
        }

        public void Deliver(Process process, Tuple<Write, Value> message) {
            _state = new Tuple<Timestamp, Value>(_ets, message.Item2);
            _bestEffortBroadcast.Broadcast((Message)new Accept());
        }

        public void Deliver(Process process, Accept accept) {
            _accepted++;
            Task.Run(() => { TryDecide(); });
        }

        private void TryDecide() {
            if (_accepted > _replicationFactor / 2) {
                _accepted = 0;
                _bestEffortBroadcast.Broadcast((Message) new Tuple<Decided, Value>(new Decided(), _tmpval));
            }
        }

        public void Deliver(Process process, Tuple<Decided, Value> message) {
            _decideListener(message.Item2);
        }

        public void Abort() {
            _abortedListener(_state);
            //TODO: find a way to halt me
        }
    }
}
