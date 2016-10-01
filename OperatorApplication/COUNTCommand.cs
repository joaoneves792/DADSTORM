using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
	class COUNTCommand : Command {
		public COUNTCommand() {
			Console.WriteLine("\t-> COUNT");
		}

		public override int Execute() {
			throw new NotImplementedException();
		}
	}
}
