using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using TupleMessage = List<String>;

    class COUNTCommand : Command {

		int count = 0;

		public COUNTCommand() {
			Console.WriteLine("\t-> COUNT");
		}

		public override TupleMessage Execute(TupleMessage inputTuple) {
			lock(this) {
				count++;
			}

			return null;
		}
	}
}
