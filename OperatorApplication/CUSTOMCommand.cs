using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
	class CUSTOMCommand : Command {
		public CUSTOMCommand() {
			Console.WriteLine("\t-> CUSTOM");
		}

		public override int Execute() {
			throw new NotImplementedException();
		}
	}
}
