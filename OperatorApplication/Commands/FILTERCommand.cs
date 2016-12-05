using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;

namespace OperatorApplication.Commands {
	using System.Collections.Concurrent;
	using TupleMessage = List<IList<string>>;

	class FILTERCommand : Command {

		private readonly Func<string, Boolean> _conditionEvalutor;
        private readonly int _fieldNumber;
        private readonly string _value;
		private Condition _condition;

        public FILTERCommand(int fieldNumber, Condition condition, string value) {

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

        private Boolean IsGreater(string value) {
            return string.Compare(value, _value) > 0;
        }

        private Boolean IsLess(string value) {
            return string.Compare(value, _value) < 0;
        }

        private Boolean IsEqual(string value) {
            return string.Compare(value, _value) == 0;
        }

        public override TupleMessage Execute(TupleMessage inputTuple) {
            TupleMessage result = new TupleMessage();

            foreach (List<string> tuple in inputTuple)
            {
                string tupleElement = tuple[_fieldNumber - 1].Replace("\"", "");
                if (_conditionEvalutor(tupleElement))
                {
                    result.Add(tuple);
                }
            }

            return (result.Count > 0) ? result : null;
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
