﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class FILTERCommand : Command {

		private readonly Func<String, Boolean> _condition;
        private readonly int _fieldNumber;
        private readonly String _value;

        public FILTERCommand(int fieldNumber, Condition condition, String value) {
            switch (condition) {
                case Condition.GREATER_THAN:
                    _condition = IsGreater;
                    break;
                case Condition.LESS_THAN:
                    _condition = IsLess;
                    break;
                case Condition.EQUALS:
                    _condition = IsEqual;
                    break;
                default:
                    throw new InvalidConditionException("Invalid condition.\r\nCondition should be '<', '>', or '='");
            }
            _fieldNumber = fieldNumber;
            _value = value;
            Console.WriteLine("\t-> FILTER");
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

            if (!_condition(tupleElement)) {
                Console.WriteLine("Tuple " + String.Join(", ", inputTuple) + " got stuck.");
                return null;
            }

            Console.WriteLine("Tuple " + String.Join(", ", inputTuple) + " has passed through filter.");
            return inputTuple;
        }

	}
}
