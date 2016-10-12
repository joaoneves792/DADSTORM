using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication.Exceptions {
	class NonExistentMethodException : System.Exception {
		public NonExistentMethodException() : base() { }
		public NonExistentMethodException(string message) : base(message) { }
	}
}
