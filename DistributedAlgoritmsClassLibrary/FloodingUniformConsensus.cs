using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using Value = IList<String>;
    using Round = Int32;

    public class FloodingUniformConsensus : UniformConsensus {
        private Action<Value> _listener;

        private const int N = 3;

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
            _listener = listener;
            _bestEffortBroadcast = new BasicBroadcast(process, Deliver, otherProcesses);
            _perfectFailureDetector = new ExcludeOnTimeout(process, Crash, otherProcesses);

            _correct = new List<Process>();
            foreach (Process otherProcess in otherProcesses) {
                _correct.Add(otherProcess);
            }
            _round = 1;
            _decision = null;
            _proposalSet = new ConcurrentBag<Value>();
            _receivedFrom = new ConcurrentBag<Process>();
        }

        public void Crash(Process process) {
            _correct.Remove(process);
            Task.Run(() => { TryDecide(); });
        }

        public void Propose(Value value) {
            _proposalSet.TryAdd(value);
            _bestEffortBroadcast.Broadcast((Message)new Tuple<Round, IProducerConsumerCollection<Value>>(1, _proposalSet));
        }

        public void Deliver(Process process, Message message) {
            Tuple<Round, IProducerConsumerCollection<Value>> tuple = (Tuple<Round, IProducerConsumerCollection<Value>>)message;

            if (tuple.Item1 != _round) {
                return;
            }

            _receivedFrom.TryAdd(process);
            _proposalSet.Concat(tuple.Item2);
            Task.Run(() => { TryDecide(); });
        }

        public void TryDecide() {
            if(_receivedFrom.Intersect(_correct).Any() && _decision == null) {
                if (_round == N) {
                    _decision = _proposalSet.GroupBy(value => value)
                                            .OrderByDescending(group => group.Count())
                                            .Select(group => group.Key)
                                            .First();
                    _listener(_decision);
                } else {
                    _round++;
                    _receivedFrom = new ConcurrentBag<Process>();
                    _bestEffortBroadcast.Broadcast((Message)new Tuple<Round, IProducerConsumerCollection<Value>>(_round, _proposalSet));
                }
            }
        }
    }
}
