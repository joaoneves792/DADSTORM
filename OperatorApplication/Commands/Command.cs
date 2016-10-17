using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using System.Collections.Concurrent;
    using TupleMessage = List<String>;

    abstract class Command {
        public abstract TupleMessage Execute(TupleMessage inputTuple);

		// FIXME: dictionary is a stupid idea
        public abstract ConcurrentDictionary<string, string> Status();
	}
}
