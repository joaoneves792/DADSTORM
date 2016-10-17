using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
	using System.Collections.Concurrent;
	using TupleMessage = List<String>;

	class DUPCommand : Command {
		public override TupleMessage Execute(TupleMessage inputTuple) {
			return inputTuple;
		}

		public override List<KeyValuePair<string, string>> Status() {

			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			return status;
		}
	}
}
