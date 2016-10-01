using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
	class FILTERCommand : Command {

		int _field_number = -1;
		char _condition = '>';
		int _value = -1;

		public FILTERCommand() {
			Console.WriteLine("\t-> DUP");
		}

		public FILTERCommand(int field_number, char cond, int value) {

			if (cond.Equals('<') || cond.Equals('>') || cond.Equals('<')) {
				_field_number = field_number;
				_condition = cond;
				_value = value;
			} else {
				// FIXME
				throw new Exception("wrong condition.");
			}

		}

		public FILTERCommand(string[] args) {
			if (args.Length == 3) {
				char cond = args[1][0];

				if (cond.Equals('<') || cond.Equals('>') || cond.Equals('<')) {
					_field_number = Int32.Parse(args[0]);
					_condition = cond;
					_value = Int32.Parse(args[2]);
				} else {
					// FIXME
					throw new Exception("wrong condition.");
				}
			} else {
				// FIXME
				throw new Exception("wrong number of args.");
			}
		}

		public override int Execute() {
			throw new NotImplementedException();
		}
	}
}
