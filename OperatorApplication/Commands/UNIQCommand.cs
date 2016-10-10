using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class UNIQCommand : Command {

		private IProducerConsumerCollection<String> _uniqueId = new ConcurrentBag<String>();
        private readonly int _fieldNumber;

		public UNIQCommand(int fieldNumber) {
            _fieldNumber = fieldNumber;

			Console.WriteLine("\t-> UNIQ");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
            String tupleElement = inputTuple[_fieldNumber];
            TupleMessage result = null;

            lock (this) {
                if (_uniqueId.Contains(tupleElement)) {
                    Console.WriteLine("Tuple " + String.Join(", ", inputTuple) + " has duplicated value.");
                } else {

                    Console.WriteLine("Tuple " + String.Join(", ", inputTuple) + " has unique value.");
                    _uniqueId.TryAdd(tupleElement);
                    result = inputTuple;
                }
            }

            return result;
        }
	}
}
