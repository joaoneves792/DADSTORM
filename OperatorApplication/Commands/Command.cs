using System.Collections.Generic;

namespace OperatorApplication.Commands
{
    using TupleMessage = List<IList<string>>;

    abstract class Command {
        public abstract TupleMessage Execute(TupleMessage inputTuple);

		public abstract List<KeyValuePair<string, string>> Status();
	}
}
