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
		private Object _obj = null;
		private MethodInfo _method = null;

		public CUSTOMCommand(string customDll, string customClass, string customMethod) {
			Console.WriteLine("\t-> CUSTOM");

			_assembly = Assembly.LoadFrom(@customDll);
			_type = _assembly.GetType(customClass);

			if (_type == null) {
				Console.WriteLine("\t-> NOPE");
				throw new NonExistentClassException(customClass + " could not be found.");
			}

			_obj = Activator.CreateInstance(_type);
			_method = _type.GetMethod(customMethod);

			if (_method == null) {
				Console.WriteLine("\t-> NOPE");
				throw new NonExistentMethodException(customClass + "." + customMethod + " could not be found.");
			}
		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
			return (TupleMessage) _method.Invoke(_obj, new object[] { inputTuple });
		}
	}
}
