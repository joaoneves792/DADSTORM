using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Exceptions {
	class InvalidConditionException : System.Exception {
		public InvalidConditionException() : base() { }
		public InvalidConditionException(string message) : base(message) { }
	}
}
