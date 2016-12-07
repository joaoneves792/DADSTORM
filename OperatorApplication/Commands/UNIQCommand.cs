using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OperatorApplication.Commands
{
    using TupleMessage = List<IList<string>>;

    class UNIQCommand : Command {

		private IProducerConsumerCollection<string> _uniqueId = new ConcurrentBag<string>();
        private readonly int _fieldNumber;

		public UNIQCommand(int fieldNumber) {
            _fieldNumber = fieldNumber;
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
            TupleMessage result = new TupleMessage();

            foreach (List<string> tuple in inputTuple) {
                string tupleElement = tuple[_fieldNumber];
                lock (_uniqueId) {
                    if (!_uniqueId.Contains(tupleElement)) {
                        _uniqueId.TryAdd(tupleElement);
                        result.Add(tuple);
                    }
                }
            }
            
            return (result.Count > 0)? result : null;
        }



		public override List<KeyValuePair<string, string>> Status() {
			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			status.Add(new KeyValuePair<string, string>("Field Number", "" + _fieldNumber));

			return status;
		}

	}
}
