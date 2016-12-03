using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Timestamp = Int32;

    public class LeaderDrivenConsensus<Value> : UniformConsensus<Value> {
        private const string CLASSNAME = "LeaderDrivenConsensus";
        private readonly EventWaitHandle _waitHandle;

        private Action<Value> _listener;
        private EpochChange _epochChange;
        private IList<EpochConsensus<Value>> _epochConsensus;
        private int _replicationFactor;
        private Process _self;
        private Process[] _processes;

        private Value _val;
        private bool _proposed,
                     _decided;
        private Tuple<Timestamp, Process> _currentLeader,
                                          _newLeader;

        public LeaderDrivenConsensus(Process process,
                              int replicationFactor,
                              Action<Value> listener,
                              params Process[] otherProcesses) {
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

            _currentLeader = new Tuple<Timestamp, Process>(0, null);
            _newLeader = new Tuple<Timestamp, Process>(0, null);

            _epochConsensus = new List<EpochConsensus<Value>>();
            _epochConsensus.Add(new ReadWriteEpochConsensus<Value>(_self.Concat(_currentLeader.Item1.ToString()),
                                                                   new Tuple<Timestamp, Value>(0, default(Value)),
                                                                   _replicationFactor,
                                                                   0,
                                                                   Decide,
                                                                   Aborted,
                                                                   _processes));
            _epochChange = new LeaderBasedEpochChange(_self, _self, StartEpoch, _processes);
            _waitHandle = new AutoResetEvent(false);
            _waitHandle.WaitOne();
        }

        public void Propose(Value value) {
            _val = value;
            Task.Run(() => { TryPropose(); });
        }

        public void StartEpoch (Timestamp timestamp, Process process) {
            _newLeader = new Tuple<Timestamp, Process>(timestamp, process);
            _epochConsensus[_currentLeader.Item1].Abort();
        }

        public void Aborted (Tuple<Timestamp, Value> state) {
            _currentLeader = _newLeader;

            Process[] suffixedProcesses = _processes
                .Select((suffixedProcess) => suffixedProcess.Concat(_currentLeader.Item1.ToString()))
                .ToArray();

            _epochConsensus.Add(new ReadWriteEpochConsensus<Value>(_self.Concat(_currentLeader.Item1.ToString()),
                                                                   state,
                                                                   _replicationFactor,
                                                                   _currentLeader.Item1,
                                                                   Decide,
                                                                   Aborted,
                                                                   suffixedProcesses));
            _proposed = false;
            Task.Run(() => { TryPropose(); });
            _waitHandle.Set();
        }

        private void TryPropose () {
            if (_currentLeader.Item2.Equals(_self) && _val != null && _proposed == false) {
                _proposed = true;
                _epochConsensus[_currentLeader.Item1].Propose(_val);
            }
        }

        public void Decide (Value value) {
            if (_decided == false) {
                _decided = true;
                _listener(value);
            }
        }
    }
}
