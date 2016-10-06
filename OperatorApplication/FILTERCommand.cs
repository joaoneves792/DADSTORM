using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    class FILTERCommand : Command {

		int _field_number = -1;
		Condition _condition = Condition.UNDEFINED;
		int _value = -1;

		public FILTERCommand() {
			Console.WriteLine("\t-> DUP");
		}

		public FILTERCommand(int field_number, Condition condition, int value) {

			if (condition == Condition.UNDEFINED) {
				// FIXME
				throw new Exception("wrong condition.");
			} else {
				_field_number = field_number;
				_condition = condition;
				_value = value;
			}

		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
            throw new NotImplementedException();
		}

	}
}
