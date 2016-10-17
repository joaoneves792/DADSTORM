using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Commands {
    using System.Collections.Concurrent;
    using TupleMessage = List<String>;

    class COUNTCommand : Command {

		int _count = 0;

		public override TupleMessage Execute(TupleMessage inputTuple) {
			lock(this) {
				_count++;
            }

            return null;
		}

        public override ConcurrentDictionary<string, string> Status() {

            ConcurrentDictionary<string, string> status = new ConcurrentDictionary<string, string>();

			status.TryAdd("Seen tuples", ""+_count);
			
            return status;
		}
    }
}
