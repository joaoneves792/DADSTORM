using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    class COUNTCommand : Command {
		public COUNTCommand() {
			Console.WriteLine("\t-> COUNT");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			throw new NotImplementedException();
		}
	}
}
