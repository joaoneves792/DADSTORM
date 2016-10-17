using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;

namespace OperatorApplication.Commands {
	using System.Collections.Concurrent;
	using TupleMessage = List<String>;

	class FILTERCommand : Command {

		private readonly Func<String, Boolean> _conditionEvalutor;
        private readonly int _fieldNumber;
        private readonly String _value;
		private Condition _condition;

        public FILTERCommand(int fieldNumber, Condition condition, String value) {

			_condition = condition;

			switch (condition) {
                case Condition.GREATER_THAN:
                    _conditionEvalutor = IsGreater;
                    break;
                case Condition.LESS_THAN:
                    _conditionEvalutor = IsLess;
                    break;
                case Condition.EQUALS:
                    _conditionEvalutor = IsEqual;
                    break;
                default:
                    throw new InvalidConditionException("Invalid condition.\r\nCondition should be '<', '>', or '='");
            }
            _fieldNumber = fieldNumber;
            _value = value;
		}

        private Boolean IsGreater(String value) {
            return String.Compare(value, _value) > 0;
        }

        private Boolean IsLess(String value) {
            return String.Compare(value, _value) < 0;
        }

        private Boolean IsEqual(String value) {
            return String.Compare(value, _value) == 0;
        }

        public override TupleMessage Execute(TupleMessage inputTuple) {
            String tupleElement = inputTuple[_fieldNumber];

            if (!_conditionEvalutor(tupleElement)) {
                return null;
            }

            return inputTuple;
        }

		public override List<KeyValuePair<string, string>> Status() {

			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			status.Add(new KeyValuePair<string, string>("Field Number", "" + _fieldNumber));
			status.Add(new KeyValuePair<string, string>("Condition", "" + _condition));
			status.Add(new KeyValuePair<string, string>("Value", "" + _value));

			return status;
		}

	}
}
