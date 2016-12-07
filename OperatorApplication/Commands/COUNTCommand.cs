using System.Collections.Generic;

namespace OperatorApplication.Commands
{
    using TupleMessage = List<IList<string>>;

    class COUNTCommand : Command {

        private object _countLock;
		int _count;

        public COUNTCommand() {
            _countLock = new object();
            _count = 0;
        }

        public override TupleMessage Execute(TupleMessage inputTuple) {
            lock (_countLock) {
                _count += inputTuple.Count;
            }

            return null;
		}


		public override List<KeyValuePair<string, string>> Status() {
			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			status.Add(new KeyValuePair<string, string>("Seen tuples", _count.ToString()));

			return status;
		}

	}
}
