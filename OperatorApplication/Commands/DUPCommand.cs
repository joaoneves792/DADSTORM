using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class DUPCommand : Command {
		public DUPCommand() {
			Console.WriteLine("\t-> FILTER");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			return inputTuple;
		}
	}
}
