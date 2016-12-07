namespace OperatorApplication.Exceptions
{
    class NonExistentMethodException : System.Exception {
		public NonExistentMethodException() : base() { }
		public NonExistentMethodException(string message) : base(message) { }
	}
}
