using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Timestamp = Int32;

    public class LeaderDrivenConsensus<Value> : UniformConsensus<Value> {
        private const string CLASSNAME = "LeaderDrivenConsensus";

        private Action<Value> _listener;
        private Action<Timestamp, Process> _epochChangeListener;
        private EpochChange _epochChange;
        private BestEffortBroadcast _bestEffortBroadcast;
        private int _replicationFactor;
        private Process _self;
        private Process[] _processes;

        private Value _val;
        private bool _proposed,
                     _decided;
        private object _proposedLock,
                       _decidedLock;
        private Tuple<Timestamp, Process> _currentLeader,
                                          _newLeader;

        public LeaderDrivenConsensus(Process process,
                                     int replicationFactor,
                                     Action<Value> listener,
                                     Action<Timestamp, Process> epochChangeListener,
                                     Process currentLeader,
                                     params Process[] otherProcesses) {
            _proposedLock = new Object();
            _decidedLock = new Object();

            _epochChangeListener = epochChangeListener;

            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _replicationFactor = replicationFactor;
            _self = process.Concat(CLASSNAME);
            _processes = suffixedProcesses;
            _listener = listener;

            _val = default(Value);
            _proposed = false;
            _decided = false;

            _currentLeader = new Tuple<Timestamp, Process>(0, currentLeader.Concat(CLASSNAME));
            _newLeader = new Tuple<Timestamp, Process>(0, null);

            Process[] suffixedEpochProcesses = _processes
                .Select((suffixedProcess) => suffixedProcess.Concat(_currentLeader.Item1.ToString()))
                .ToArray();

            _bestEffortBroadcast = new BasicBroadcast(_self, Decide, _processes);
            _epochChange = new LeaderBasedEpochChange(_self, _currentLeader.Item2, StartEpoch, suffixedProcesses);
        }

        public void Propose(Value value) {
            _val = value;
            TryPropose();
        }

        public void StartEpoch (Timestamp timestamp, Process process) {
            _currentLeader = new Tuple<Timestamp, Process>(timestamp, process);
            _epochChangeListener(timestamp, process);
        }

        private void TryPropose () {
            lock (_proposedLock) {
                if (!(/*_currentLeader.Item2.Equals(_self) && */_val != null && !_proposed)) {
                    return;
                }
                _proposed = true;
            }

            _bestEffortBroadcast.Broadcast(_val);
        }

        public void Decide (Process process, object value) {
            lock (_decidedLock) {
                if (_decided) {
                    return;
                }
                _decided = true;
            }

            _listener((Value) value);
        }
    }
}
