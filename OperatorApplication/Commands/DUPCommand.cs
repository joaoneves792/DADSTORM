using System.Collections.Generic;

namespace OperatorApplication.Commands
{
    using TupleMessage = List<IList<string>>;

    class DUPCommand : Command {
		public override TupleMessage Execute(TupleMessage inputTuple) {
			return inputTuple;
		}

		public override List<KeyValuePair<string, string>> Status() {

			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			return status;
		}
	}
}
