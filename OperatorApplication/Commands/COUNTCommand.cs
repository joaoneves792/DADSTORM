using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using System.Collections.Concurrent;
    using TupleMessage = List<IList<String>>;

    class COUNTCommand : Command {

		int _count = 0;

		public override TupleMessage Execute(TupleMessage inputTuple) {
            foreach (List<String> tuple in inputTuple)
            {
                lock (this)
                {
                    _count++;
                }
            }

            return null;
		}


		public override List<KeyValuePair<string, string>> Status() {

			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			status.Add(new KeyValuePair<string, string>("Seen tuples", ""+_count));

			return status;
		}

	}
}
