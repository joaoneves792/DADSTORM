using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public class RandomBroadcast : ReliableBroadcast {
        private const string CLASSNAME = "ReliableBroadcast";
        private FairLossPointToPointLink _fairLossPointToPointLink;

        private IDictionary<string, IList<Process>> _correct;

        public IList<Process> Processes {
            get {
                return _correct
                    .Values
                    .Aggregate((list, next) => list.Concat(next).ToList());
            }
        }

        private IEnumerable<Process> Correct {
            get {
                return _correct
                    .Values
                    .Select((list) => list[new Random().Next(list.Count)]);
            }
        }

        public RandomBroadcast(Process process, Action<Process, Message> listener) {
            _correct = new Dictionary<string, IList<Process>>();
            _fairLossPointToPointLink = new RemotingNode(process.Concat(CLASSNAME), listener);
        }

        public void Broadcast(Message message) {
            Parallel.ForEach(Correct, process => {
                _fairLossPointToPointLink.Send(process, message);
            });
        }

        public void Connect(Process process) {
            Process suffixedProcess = process.Concat(CLASSNAME);

            lock (_correct) {
                //Create new list if list does not exist
                if (!_correct.ContainsKey(suffixedProcess.Name)) {
                    _correct.Add(suffixedProcess.Name, new List<Process>() { suffixedProcess });
                    _fairLossPointToPointLink.Connect(suffixedProcess);
                    return;
                }

                //Add process if process is not on list (list already exists)
                if (!_correct[suffixedProcess.Name].Contains(suffixedProcess)) {
                    _correct[suffixedProcess.Name].Add(suffixedProcess);
                    _fairLossPointToPointLink.Connect(suffixedProcess);
                }
            }
        }
    }
}
