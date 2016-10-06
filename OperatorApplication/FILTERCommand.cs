using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    class FILTERCommand : Command {

		// segundo o luis, field_number is the first element of the tuple so this is wrong
		//int _field_number = -1;
		Condition _condition = Condition.UNDEFINED;
		int _value = -1;

		public FILTERCommand() {
			Console.WriteLine("\t-> DUP");
		}

		public FILTERCommand(int field_number, Condition condition, int value) {

			if (condition == Condition.UNDEFINED) {
				// FIXME: create right exception
				throw new Exception("wrong condition.");
			} else {
				//_field_number = field_number;
				_condition = condition;
				_value = value;
			}

		}


		public override TupleMessage Execute(TupleMessage inputTuple) {

			int field_number = Int32.Parse(inputTuple.First());

			if (   ((_condition == Condition.LESS_THAN) && (field_number < _value))
				|| ((_condition == Condition.GREATER_THAN) && (field_number > _value))
				|| ((_condition == Condition.EQUALS) && (field_number == _value))
				)
				return inputTuple;

			return null;

		}

	}
}
