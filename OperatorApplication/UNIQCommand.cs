using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    class UNIQCommand : Command {

		ConcurrentDictionary<int, int> _field_numbers = new ConcurrentDictionary<int, int>();

		public UNIQCommand() {
			Console.WriteLine("\t-> UNIQ");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			int field_number = Int32.Parse(inputTuple.First());

			if (!_field_numbers.ContainsKey(field_number)) {
				_field_numbers.TryAdd(field_number, field_number);
				return inputTuple;
			}

			return null;
		}
	}
}
