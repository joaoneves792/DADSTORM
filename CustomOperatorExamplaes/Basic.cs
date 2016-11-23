using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomOperatorExamplaes {
    public class Basic {

		public Basic() { }

		public List<List<String>> Repeat(List<String> input) {
            List<List<String>> result = new List<List<string>>();
            result.Add(input);
            return result;
		}


    }
}
