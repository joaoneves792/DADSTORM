namespace OperatorApplication.Exceptions
{
    class InvalidConditionException : System.Exception {
		public InvalidConditionException() : base() { }
		public InvalidConditionException(string message) : base(message) { }
	}
}
