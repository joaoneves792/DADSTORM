using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Exceptions {
	class NonExistentClassException : System.Exception {
		public NonExistentClassException() : base() { }
		public NonExistentClassException(string message) : base(message) { }
	}
}
