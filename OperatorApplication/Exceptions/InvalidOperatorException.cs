using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Exceptions {
	class InvalidOperatorException : System.Exception {
		public InvalidOperatorException() : base() { }
		public InvalidOperatorException(string message) : base(message) { }
	}
}
