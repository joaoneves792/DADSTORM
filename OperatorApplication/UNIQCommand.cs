using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    class UNIQCommand : Command {

		int _field_number = -1;

		public UNIQCommand() {
			Console.WriteLine("\t-> UNIQ");
		}

		public UNIQCommand(int field_number) {
			_field_number = field_number;
		}

		public UNIQCommand(string[] args) {
			if (args.Length == 1) {
				_field_number = Int32.Parse(args[0]);

			} else {
				// FIXME
				throw new Exception("wrong number of args.");
			}
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			throw new NotImplementedException();
		}
	}
}
