using OperatorApplication.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace OperatorApplication.Commands
{
    using TupleMessage = List<IList<string>>;

    class CUSTOMCommand : Command {

		private Assembly _assembly = null;
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

            foreach (Type type in _assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + customClass))
                    {
                        // create an instance of the object
                        _obj = Activator.CreateInstance(type);

                        _methodInfo = type.GetMethod(customMethod);
                    }
                }
            }
            
			if (_obj == null) {
				throw new NonExistentClassException(customClass + " could not be found.");
			}

			if (_methodInfo == null) {
				throw new NonExistentMethodException(customClass + "." + customMethod + " could not be found.");
			}
		}


		public override TupleMessage Execute(TupleMessage inputTuple) {
            TupleMessage result = new TupleMessage();
            foreach (List<string> tuple in inputTuple)
            {
                result.AddRange((TupleMessage)_methodInfo.Invoke(_obj, new object[] { tuple }));
            }
            return (result.Count > 0) ? result : null;
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
