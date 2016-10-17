﻿using System;
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
		private MethodInfo _method = null;

		public CUSTOMCommand(string customDll, string customClass, string customMethod) {
			_assembly = Assembly.LoadFrom(@customDll);
			_type = _assembly.GetType(customClass);

			if (_type == null) {
				throw new NonExistentClassException(customClass + " could not be found.");
			}

			_obj = Activator.CreateInstance(_type);
			_method = _type.GetMethod(customMethod);

			if (_method == null) {
				throw new NonExistentMethodException(customClass + "." + customMethod + " could not be found.");
			}
		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
			return (TupleMessage) _method.Invoke(_obj, new object[] { inputTuple });
		}

		public override ConcurrentDictionary<string, string> Status() {

			ConcurrentDictionary<string, string> status = new ConcurrentDictionary<string, string>();

			status.TryAdd("Library", "hello");
			status.TryAdd("Class", "there");
			status.TryAdd("Method", "is it me");

			return status;
		}
	}
}
