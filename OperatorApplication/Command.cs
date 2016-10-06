using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication {
    using TupleMessage = List<String>;

    abstract class Command {

		public Command() { }

		public abstract TupleMessage Execute(TupleMessage inputTuple);
	}
}
