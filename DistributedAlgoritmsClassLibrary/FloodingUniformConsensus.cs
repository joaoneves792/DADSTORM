using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using Round = Int32;

    public class FloodingUniformConsensus<Value> : UniformConsensus<Value> {
        private Action<Value> _listener;

        private const int N = 3;
        private const string CLASSNAME = "FloodingUniformConsensus";
        private BestEffortBroadcast _bestEffortBroadcast;
        private PerfectFailureDetector _perfectFailureDetector;

        private IList<Process> _correct;
        private Round _round;
        private Value _decision;
        private IProducerConsumerCollection<Value> _proposalSet;
        private IProducerConsumerCollection<Process> _receivedFrom;

        public FloodingUniformConsensus(Process process,
                                        Action<Value> listener,
                                        params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _listener = listener;

            _correct = new List<Process>();
            _correct.Add(process.Concat(CLASSNAME));
            foreach (Process otherProcess in suffixedProcesses) {
                _correct.Add(otherProcess);
            }
            _round = 1;
            _decision = default(Value);
            _proposalSet = new ConcurrentBag<Value>();
            _receivedFrom = new ConcurrentBag<Process>();

            _perfectFailureDetector = new ExcludeOnTimeout(process.Concat(CLASSNAME), Crash, suffixedProcesses);
            _bestEffortBroadcast = new BasicBroadcast(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
        }

        public void Crash(Process process) {
            _correct.Remove(process);
            TryDecide();
        }

        public void Propose(Value value) {
            _proposalSet.TryAdd(value);
            _bestEffortBroadcast.Broadcast((Message)new Tuple<Round, IProducerConsumerCollection<Value>>(1, _proposalSet));
        }

        public void Deliver(Process process, Message message) {
            Tuple<Round, IProducerConsumerCollection<Value>> tuple = (Tuple<Round, IProducerConsumerCollection<Value>>)message;

            if (tuple.Item1 < _round) {
                return;
            }

            if (tuple.Item1 > _round) {
                _round = tuple.Item1;
            }
            _receivedFrom.TryAdd(process);
            foreach (Value value in tuple.Item2) {
                _proposalSet.TryAdd(value);
            }
            TryDecide();
        }

        public void TryDecide() {
            int round;

            // Check if received messages from any correct process
            lock (_receivedFrom) {
                if (!(_receivedFrom.Intersect(_correct).Any() && _decision == null)) {
                    return;
                }
                if (_round == N) {
                    _decision = _proposalSet.GroupBy(value => value)
                                                .OrderByDescending(group => group.Count())
                                                .Select(group => group.Key)
                                                .First();
                }

                _receivedFrom = new ConcurrentBag<Process>();
                round = ++_round;
            }

            // Return result if the consensus reaches round N
            if (round == N + 1) {
                _listener(_decision);
            // Go to next round
            } else {
                _bestEffortBroadcast.Broadcast((Message)new Tuple<Round, IProducerConsumerCollection<Value>>(round, _proposalSet));
            }
        }
    }
}
