using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;


namespace OperatorApplication.Commands {
	using TupleMessage = List<String>;

	class CUSTOMCommand : Command {

		private Assembly _assembly = null;
		private Type _type = null;
		private Object obj = null;
		private MethodInfo method = null;

		public CUSTOMCommand(string customDll, string customClass, string customMethod) {
			Console.WriteLine("\t-> CUSTOM");

			_assembly = Assembly.LoadFrom(@customDll);
			_type = _assembly.GetType(customClass);
			obj = Activator.CreateInstance(_type);
			method = _type.GetMethod(customMethod);

			if (method == null) {
				Console.WriteLine("\t-> CUSTOM");
				throw new NonExistentMethodException(customClass + "." + customMethod + "could not be found.");
			}
		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
			return (TupleMessage) method.Invoke(obj, new object[] { inputTuple });
		}
	}
}
