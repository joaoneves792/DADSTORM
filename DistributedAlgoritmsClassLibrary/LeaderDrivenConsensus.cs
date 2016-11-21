﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Value = IList<String>;
    using Timestamp = Int32;

    public class LeaderDrivenConsensus : UniformConsensus {
        private readonly EventWaitHandle _waitHandle;

        private Action<Value> _listener;
        private EpochChange _epochChange;
        private IList<EpochConsensus> _epochConsensus;
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
            _waitHandle = new AutoResetEvent(false);

            _replicationFactor = replicationFactor;
            _self = process;
            _processes = otherProcesses;
            _listener = listener;
            _epochChange = new LeaderBasedEpochChange(_self, _self, StartEpoch, _processes);
            _epochConsensus = new List<EpochConsensus>();
            _epochConsensus.Add(new ReadWriteEpochConsensus(_self,
                                                            new Tuple<Timestamp, Value>(0, null),
                                                            _replicationFactor,
                                                            0,
                                                            Decide,
                                                            Aborted,
                                                            _processes));
            _waitHandle.WaitOne();

            _val = null;
            _proposed = false;
            _decided = false;


            _currentLeader = new Tuple<Timestamp, Process>(0, null);
            _newLeader = new Tuple<Timestamp, Process>(0, null);
        }

        public void Propose(Value value) {
            _val = value;
        }

        public void StartEpoch (Timestamp timestamp, Process process) {
            _newLeader = new Tuple<Timestamp, Process>(timestamp, process);
            _epochConsensus[_currentLeader.Item1].Abort();
        }

        public void Aborted (Tuple<Timestamp, Value> state) {
            _currentLeader = _newLeader;
            Task.Run(() => { TryPropose(); });
            _proposed = false;
            _waitHandle.Set();

            _epochConsensus.Add(new ReadWriteEpochConsensus(_self,
                                                            state,
                                                            _replicationFactor,
                                                            _currentLeader.Item1,
                                                            Decide,
                                                            Aborted,
                                                            _processes));
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
