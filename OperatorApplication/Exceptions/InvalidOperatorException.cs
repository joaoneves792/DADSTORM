namespace OperatorApplication.Exceptions
{
    class InvalidOperatorException : System.Exception {
		public InvalidOperatorException() : base() { }
		public InvalidOperatorException(string message) : base(message) { }
	}
}
