using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using NewTimestamp = Int32;

    class LeaderBasedEpochChange : EpochChange {
        private Action<NewTimestamp, Process> _listener;
        private PerfectPointToPointLink _perfectPointToPointLink;
        private BestEffortBroadcast _bestEffortBroadcast;
        private EventualLeaderDetector _eventualLeaderDetector;
        private const int N = 10;

        private Process _trusted,
                        _self;
        private int _lastts;

        public LeaderBasedEpochChange(Process process,
                                      Action<NewTimestamp, Process> listener) {
            _listener = listener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver);
            _bestEffortBroadcast = new BasicBroadcast(process, Deliver);
            _eventualLeaderDetector = new MonarchicalEventualLeaderDetection(process, Trust);

            //FIXME: Anarchic first epoch alert!!!!!
            _trusted = process;
            _self = process;
            _lastts = 0;
        }

        public LeaderBasedEpochChange(Process process,
                                      Action<NewTimestamp, Process> listener,
                                      params Process[] otherProcesses) {
            _listener = listener;
            _perfectPointToPointLink = new EliminateDuplicates(process, Deliver, otherProcesses);
            _bestEffortBroadcast = new BasicBroadcast(process, Deliver, otherProcesses);
            _eventualLeaderDetector = new MonarchicalEventualLeaderDetection(process, Trust, otherProcesses);

            //FIXME: Anarchic first epoch alert!!!!!
            _trusted = process;
            _self = process;
            _lastts = 0;
        }

        public void Trust(Process process) {
            _trusted = process;
            if (process.Equals(_self)) {
                _self.Rank += N;
                _bestEffortBroadcast.Broadcast((Message) new Tuple<NewEpoch, NewTimestamp>(new NewEpoch(), _self.Rank));
            }
        }

        public void Deliver(Process process, Message message) {
            if (message is Tuple<NewEpoch, NewTimestamp>) {
                Deliver(process, (Tuple<NewEpoch, NewTimestamp>)message);
            } else if (message is Nack) {
                Deliver(process, (Nack)message);
            }
        }

        public void Deliver(Process process, Tuple<NewEpoch, NewTimestamp> message) {
            int newts = message.Item2;
            if (process.Equals(_trusted) && newts > _lastts) {
                _lastts = newts;
                _listener(newts, process);
            } else {
                _perfectPointToPointLink.Send(process, (Message)new Nack());
            }
        }

        public void Deliver(Process process, Nack nack) {
            if(_trusted.Equals(_self)) {
                _self.Rank += N;
                _bestEffortBroadcast.Broadcast((Message)new Tuple<NewEpoch, NewTimestamp>(new NewEpoch(), _self.Rank));
            }
        }

        public void Connect(Process process) {
            _perfectPointToPointLink.Connect(process);
            _bestEffortBroadcast.Connect(process);
            _eventualLeaderDetector.Connect(process);
        }
    }
}
