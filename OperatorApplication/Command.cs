using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
	abstract class Command {

		public Command() { }

		public abstract int Execute();
	}
}
