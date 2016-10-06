using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class FILTERCommand : Command {

		Condition _condition = Condition.UNDEFINED;
		int _value = -1;

		public FILTERCommand() {
			Console.WriteLine("\t-> DUP");
		}

		public FILTERCommand(int field_number, Condition condition, int value) {

			if (condition == Condition.UNDEFINED) {
				throw new InvalidConditionException("Invalid condition.\r\nCondition should be '<', '>', or '='");
			} else {
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
