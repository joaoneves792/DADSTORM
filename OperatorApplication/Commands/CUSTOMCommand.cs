using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class CUSTOMCommand : Command {
		public CUSTOMCommand() {
			Console.WriteLine("\t-> CUSTOM");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			throw new NotImplementedException();
		}
	}
}
