using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using System.Collections.Concurrent;
    using TupleMessage = List<IList<String>>;

    abstract class Command {
        public abstract TupleMessage Execute(TupleMessage inputTuple);

		public abstract List<KeyValuePair<string, string>> Status();
	}
}
