using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using TupleMessage = List<IList<String>>;

    class UNIQCommand : Command {

		private IProducerConsumerCollection<String> _uniqueId = new ConcurrentBag<String>();
        private readonly int _fieldNumber;

		public UNIQCommand(int fieldNumber) {
            _fieldNumber = fieldNumber;
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
            TupleMessage result = new TupleMessage();

            foreach (List<String> tuple in inputTuple)
            {
                String tupleElement = tuple[_fieldNumber];
                lock (this)
                {
                    if (!_uniqueId.Contains(tupleElement))
                    {
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
