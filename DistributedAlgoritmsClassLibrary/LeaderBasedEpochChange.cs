using System;
using System.Linq;
using System.Threading;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using Timestamp = Int32;

    public class LeaderBasedEpochChange : EpochChange {
        private Action<Timestamp, Process> _listener;
        private PerfectPointToPointLink _perfectPointToPointLink;
        private BestEffortBroadcast _bestEffortBroadcast;
        private EventualLeaderDetector _eventualLeaderDetector;
        private const int N = 1;
        private const string CLASSNAME = "LeaderBasedEpochChange";
        private Process _trusted,
                        _self;
        private int _lastts;

        public LeaderBasedEpochChange(Process process,
                                      Process leader,
                                      Action<Timestamp, Process> listener,
                                      params Process[] otherProcesses) {
            Process[] suffixedProcesses = otherProcesses
                .Select((suffixedProcess) => suffixedProcess.Concat(CLASSNAME))
                .ToArray();

            _listener = listener;

            _trusted = leader;
            _self = process.Concat(CLASSNAME);
            _lastts = 0;

            _perfectPointToPointLink = new EliminateDuplicates(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
            _bestEffortBroadcast = new BasicBroadcast(process.Concat(CLASSNAME), Deliver, suffixedProcesses);
            _eventualLeaderDetector = new MonarchicalEventualLeaderDetection(process.Concat(CLASSNAME), Trust, suffixedProcesses);
        }

        public void Trust(Process process) {
            _trusted = process;
            if (_trusted.Equals(_self)) {
                _self.Rank += N;
                _bestEffortBroadcast.Broadcast((Message)_self.Rank);
            }
        }

        public void Deliver(Process process, Message message) {
            if (message is Timestamp) {
                DeliverNewEpoch(process, (Timestamp)message);
            } else if (message is Signal) {
                DeliverNack(process);
            }
        }

        public void DeliverNewEpoch(Process process, Timestamp message) {
            int newts = message;
            if (process.Equals(_trusted) && newts > _lastts) {
                _lastts = newts;
                _listener(newts, process.Unconcat(CLASSNAME));
            } else {
            }
        }

        public void DeliverNack(Process process) {
            if (_trusted.Equals(_self)) {
                _self.Rank += N;
                _bestEffortBroadcast.Broadcast((Message)new Tuple<Signal, Timestamp>(Signal.NEW_EPOCH, _self.Rank));
            }
        }
    }
}
