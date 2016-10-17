using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OperatorApplication.Exceptions;


namespace OperatorApplication.Commands {
	using System.Collections.Concurrent;
	using TupleMessage = List<String>;

	class CUSTOMCommand : Command {

		private Assembly _assembly = null;
		private Type _type = null;
		private Object _obj = null;
		private MethodInfo _methodInfo = null;

		private string _dll = null;
		private string _class = null;
		private string _method = null;

		public CUSTOMCommand(string customDll, string customClass, string customMethod) {

			_dll = customDll;
			_class = customClass;
			_method = customMethod;

			_assembly = Assembly.LoadFrom(@customDll);
			_type = _assembly.GetType(customClass);

			if (_type == null) {
				throw new NonExistentClassException(customClass + " could not be found.");
			}

			_obj = Activator.CreateInstance(_type);
			_methodInfo = _type.GetMethod(customMethod);

			if (_method == null) {
				throw new NonExistentMethodException(customClass + "." + customMethod + " could not be found.");
			}
		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
			return (TupleMessage)_methodInfo.Invoke(_obj, new object[] { inputTuple });
		}


		public override List<KeyValuePair<string, string>> Status() {

			List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();

			status.Add(new KeyValuePair<string, string>("Library", "" + _dll));
			status.Add(new KeyValuePair<string, string>("Class", "" + _class));
			status.Add(new KeyValuePair<string, string>("Method", "" + _method));

			return status;
		}

	}
}
