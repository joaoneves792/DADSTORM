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

		public override ConcurrentDictionary<string, string> Status() {

			ConcurrentDictionary<string, string> status = new ConcurrentDictionary<string, string>();

			return status;
		}
	}
}
