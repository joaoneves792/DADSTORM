namespace OperatorApplication.Exceptions
{
    class NonExistentClassException : System.Exception {
		public NonExistentClassException() : base() { }
		public NonExistentClassException(string message) : base(message) { }
	}
}
